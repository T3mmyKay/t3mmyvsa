# Process Topology

T3mmyVSA separates the HTTP API, database migration runner and Hangfire worker into independently deployable processes when the hosting environment supports it.

```text
                 ┌──────────────────┐
HTTP requests ──▶│       API        │
                 │ enqueue/dashboard│
                 └────────┬─────────┘
                          │ Hangfire storage
                          ▼
                 ┌──────────────────┐
                 │      Worker      │
                 │ processes queues │
                 └──────────────────┘

Deployment ─────▶ Migrations executable ─────▶ application database
                  (run-to-completion)
```

## API

`T3mmyvsa.csproj` owns HTTP endpoints, authentication/authorization, API documentation and health probes. Hangfire storage/client services remain available so endpoints and services can enqueue work, but `Hangfire:ServerEnabled` defaults to `false`.

## Worker

`Worker/T3mmyvsa.Worker.csproj` is a Generic Host that references the application assembly and reuses its database, services, mediator and Hangfire configuration. It forces `Hangfire:ServerEnabled=true` and disables the dashboard.

Why separate it:

- CPU/memory-heavy jobs cannot directly compete with the API process for the same process resources;
- web deployments/restarts do not intentionally stop job execution when workers are deployed independently;
- worker replicas, queues and `WorkerCount` can be scaled and tuned independently;
- API replicas do not accidentally multiply worker concurrency.

This is process isolation and operational control, not a guarantee that an individual job executes faster.

## Migrations

`Migrations/T3mmyvsa.Migrations.csproj` owns the EF migration assembly and design-time factory. Its run-to-completion executable applies migrations with bounded startup retry/backoff.

Benefits:

- EF tooling does not need to boot the web host;
- provider-specific migrations have one explicit owner;
- deployment images can use the ASP.NET runtime rather than shipping the SDK and `dotnet-ef`;
- migration failure blocks API/worker startup in the generated Compose topology.

Generate migrations with the helper scripts or target the migrations project explicitly. Do not place migrations back under the web project.

## Deployment modes

### Docker, VPS and orchestrated hosting — recommended

Use the full split topology:

- API: one or more replicas with `Hangfire:ServerEnabled=false`;
- Worker: one or more independently managed replicas;
- Migrations: one run-to-completion execution per deployment.

This is the preferred mode for applications with important, long-running or resource-heavy background processing.

### SmarterASP.NET shared hosting

SmarterASP.NET shared ASP.NET Core hosting is a single IIS-managed web application rather than a general-purpose multi-process host. The generated `deploy-smarteraspnet.yml` therefore uses a compatibility mode:

1. build the complete solution so API, Worker and Migrations remain compiler-checked;
2. run `T3mmyvsa.Migrations` from GitHub Actions against the configured SQL Server before deployment;
3. publish only the web application through MSDeploy;
4. set the non-secret web runtime switches `Hangfire__Enabled=true` and `Hangfire__ServerEnabled=true` in the published `web.config` so that the API process also executes Hangfire jobs.

Do not deploy the dedicated Worker as a second persistent process on ordinary shared hosting unless your hosting plan explicitly provides a supported mechanism for that process lifetime.

Sensitive runtime configuration is intentionally **not** written into `web.config`. Configure values such as `ConnectionStrings__appDatabase`, `JwtSettings__Secret`, mail credentials and bootstrap-admin credentials through the hosting control panel/application-pool environment variables or another supported secret mechanism.

Because shared IIS hosting can recycle or idle the application pool, in-process background execution does not provide the same lifetime isolation as a dedicated Worker. Hangfire's persistent storage protects queued job state, but job execution can pause while the web process is unavailable. For strict timing, long-running jobs or workloads that must keep processing independently of web traffic, use a VPS/dedicated/orchestrated deployment and run `T3mmyvsa.Worker` separately.

### Other small/single-process hosts

You may intentionally omit the dedicated Worker and set `Hangfire:ServerEnabled=true` on the API when the host only supports one persistent application process. Never run both the API worker and a dedicated Worker unintentionally unless the combined concurrency is desired.

## Shared configuration

API, Worker and Migrations must use the same application provider/connection contract. Hangfire can still use its independent SQL Server/PostgreSQL storage configuration.

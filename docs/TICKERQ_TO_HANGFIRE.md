# TickerQ to Hangfire migration note

The starter template no longer contains TickerQ packages, runtime setup, sample jobs or TickerQ EF migrations. New projects use Hangfire with SQL Server storage.

For an existing deployed application that previously used TickerQ:

1. Inventory scheduled/recurring TickerQ jobs and confirm which ones must be recreated in Hangfire.
2. Stop creating new TickerQ jobs and allow or explicitly handle any pending work.
3. Back up the database before changing scheduler infrastructure.
4. Deploy the Hangfire-enabled application with a valid Hangfire SQL Server connection and verify `/health/ready`.
5. Re-register recurring schedules in Hangfire and verify successful execution from the Hangfire dashboard/monitoring APIs.
6. Keep the old `ticker` schema temporarily if historical state is required for operational verification.
7. Only after confirming no rollback or pending-job requirement remains should an operator remove the obsolete TickerQ schema/tables manually.

Do not automatically drop an existing production TickerQ schema during application startup or EF migration. Scheduler-state deletion is an operationally destructive action and should be explicit.

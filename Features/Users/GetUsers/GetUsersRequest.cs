using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Features.Users.GetUsers;

public class GetUsersRequest : PaginationRequest
{
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    [FromQuery(Name = "sort_column")]
    [System.ComponentModel.DefaultValue(UserSortColumn.UserName)]
    public UserSortColumn? SortColumn { get; set; } = UserSortColumn.UserName;

    [FromQuery(Name = "sort_order")]
    [System.ComponentModel.DefaultValue(Models.Shared.SortOrder.Asc)]
    public SortOrder? SortOrder { get; set; } = Models.Shared.SortOrder.Asc;
}

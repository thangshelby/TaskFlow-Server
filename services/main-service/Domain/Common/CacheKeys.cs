using MainService.Domain.Interfaces;

namespace MainService.Domain.Common;

public static class CacheKeys
{
    public static class Issues
    {
        private const string Prefix = "issues";

        public static string List(GetIssuesParams param) 
        {
            var keyParts = new List<string>
            {
                "issues:list",
                $"pid:{param.ProjectId ?? "all"}",
                $"page:{param.Page}",
                $"limit:{param.Limit}",
                $"keyword:{param.Keyword ?? "none"}",
            };
            if (param.ColumnIds != null && param.ColumnIds.Any())
            {
                keyParts.Add($"cols:{string.Join(",", param.ColumnIds.OrderBy(x => x))}");
            }

            if (param.AssigneeIds != null && param.AssigneeIds.Any())
            {
                keyParts.Add($"assignees:{string.Join(",", param.AssigneeIds.OrderBy(x => x))}");
            }

            if (param.SprintIds != null && param.SprintIds.Any())
            {
                keyParts.Add($"sprints:{string.Join(",", param.SprintIds.OrderBy(x => x))}");
            }

            if (param.Types != null && param.Types.Any())
            {
                keyParts.Add($"types:{string.Join(",", param.Types.OrderBy(x => x))}");
            }

            if (param.Priorities != null && param.Priorities.Any())
            {
                keyParts.Add($"priorities:{string.Join(",", param.Priorities.OrderBy(x => x))}");
            }

            if (param.TeamIds != null && param.TeamIds.Any())
            {
                keyParts.Add($"teams:{string.Join(",", param.TeamIds.OrderBy(x => x))}");
            }

            if (param.ParentIds != null && param.ParentIds.Any())
            {
                keyParts.Add($"parents:{string.Join(",", param.ParentIds.OrderBy(x => x))}");
            }

            return string.Join(":", keyParts);
        }
           

        public static string Detail(string issueId) 
            => $"{Prefix}:detail:{issueId}";
    }

    public static class Users
    {
        public static string Profile(string userId) => $"users:profile:{userId}";
    }
}
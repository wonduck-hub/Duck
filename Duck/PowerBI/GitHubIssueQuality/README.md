# GitHub Issue Quality Power BI Project

This PBIP project builds a software-quality dashboard from GitHub issues for:

- Owner: `wonduck-hub`
- Repository: `Duck`

Open `GitHubIssueQuality.pbip` in Power BI Desktop.

## Parameters

Set these in Power BI Desktop under **Transform data > Manage Parameters**:

- `GitHubOwner`: GitHub organization/user. Default: `wonduck-hub`
- `GitHubRepo`: repository name. Default: `Duck`
- `GitHubToken`: GitHub personal access token. Enter it as a Power BI parameter; do not edit `model.bim` to paste a token.
- `GitHubPageSize`: GitHub API page size. Default: `100`
- `GitHubMaxPages`: maximum issue pages to request. Default: `20`
- `StaleIssueDays`: open issue stale threshold. Default: `30`

The token is intentionally read through the `GitHubToken` parameter. The query builds its `Authorization: Bearer ...` header from that parameter at refresh time.

## Data Source Credentials

Because the token is supplied by the `GitHubToken` parameter and converted into an HTTP `Authorization` header in Power Query, set the Power BI data source credential for `https://api.github.com` to **Anonymous**.

In Power BI Desktop:

1. Open **File > Options and settings > Data source settings**.
2. Delete any existing permission entries for `https://api.github.com/repos/...`.
3. Select or create the root entry `https://api.github.com`.
4. Choose **Edit Permissions**.
5. Set **Credentials** to **Anonymous**.
6. Refresh the model again.

If the credential dialog offers multiple scope levels, choose `https://api.github.com`, not `https://api.github.com/repos/wonduck-hub`.

## Included Tables

- `Issues`: one row per GitHub issue, excluding pull requests.
- `Calendar`: date table related to `Issues[CreatedDate]`.
- `LabelSummary`: grouped label counts for label-quality analysis.

## Included Measures

- `Total Issues`
- `Open Issues`
- `Closed Issues`
- `Close Rate`
- `Bug Issues`
- `Open Bugs`
- `Bug Share`
- `Stale Open Issues`
- `Stale Open Rate`
- `Untriaged Issues`
- `Untriaged Rate`
- `Assigned Issues`
- `Assignment Rate`
- `Avg Open Age Days`
- `Avg Time To Close Days`
- `Median Time To Close Days`
- `Quality Risk Score`

## Recommended Report Pages

### 1. Quality Overview

Use KPI cards across the top:

- `Quality Risk Score`
- `Open Issues`
- `Open Bugs`
- `Stale Open Issues`
- `Untriaged Rate`
- `Avg Time To Close Days`

Use charts below:

- Line chart: `Calendar[Date]` by `Total Issues`, filtered by created date.
- Donut chart: `Issues[State]` count.
- Bar chart: `Issues[Severity]` by `Total Issues`.

### 2. Triage Health

Use:

- Stacked bar: `Issues[IsUntriaged]` by `Issues[State]`.
- Bar chart: `LabelSummary[Label]` by `LabelSummary[Issue Count]`.
- Table: `Number`, `Title`, `State`, `LabelNames`, `AssigneeCount`, `DaysSinceUpdate`, `HtmlUrl`.

Sort the table by `DaysSinceUpdate` descending to show stale work first.

### 3. Bug And Closure Flow

Use:

- Line chart: `Calendar[Date]` by `Bug Issues`.
- Card: `Bug Share`.
- Card: `Median Time To Close Days`.
- Scatter chart: `AgeDays` vs `CommentCount`, legend by `Severity`.

## Quality Interpretation

Lower `Quality Risk Score` is better. The score is a weighted blend of:

- open bug pressure
- stale open issue rate
- untriaged issue rate
- unresolved issue rate

It is not a universal truth metric; treat it as a trend and discussion starter.

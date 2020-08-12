# Query Workbench
Prototype alternative/complement to SQL Server Management Studio for viewing query results. Currently supports SQL Server only but designed to work with other databases.

Reasons why I prefer to use this tool when investigating application data-related issues:
* Query results are displayed in tabs instead of stacked panes in SSMS; this makes it easier to focus on data returned by each query.
* Query results tabs can be renamed; comes in handy when working with queries that return multiple resultsets. SSMS query results are virtually meaningless unless one returns to the query definitions for discernment. 
* Query parameters are defined in their own panes. In SSMS, one can define all query parameters at the top of the script file but this can become unwieldy with multiple statements because one has to scroll through the query window just to modify the parameter values. In Query Workbench, there's a separate pane next to the query window where parameter values can be conveniently edited without scrolling through the query statements.
* Support for resultset filters. Oftentimes, one needs to further limit the returned data which can be achieved by altering the query but a more convenient approach is to be able to set temporary filters without altering the queries themselves.
* Hide/show columns temporarily; the concept is similar to result filters but applies to columns instead of rows.
* Ad-hoc sorting by clicking on column headers; native support provider by Windows Forms GridView.
* Support for inspecting/copying lengthy field values. In SSMS, field values are truncated after they reach a certain length and cannot be copied to the Clipboard. The workaround is to switch to Results-to-Grid, run the query, save the grid results to CSV, open the CSV and then do the copy. In Query Workbench, the value of a selected cell (regardless of length) is displayed in the Output pane where one can easily copy using standard keyboard shortcuts. This is actually the primary motivator of why I created this tool in the first place when I had to work with .NET types serialized to varchar(max) columns.

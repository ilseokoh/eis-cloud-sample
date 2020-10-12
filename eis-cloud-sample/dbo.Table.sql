CREATE TABLE [dbo].[Table]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [DetectedTime] DATETIME2 NOT NULL, 
    [DeviceId] NVARCHAR(MAX) NOT NULL, 
    [ModuleId] NVARCHAR(MAX) NOT NULL, 
    [tl_x] INT NULL, 
    [tl_y] INT NULL, 
    [br_x] INT NULL, 
    [br_y] INT NULL, 
)

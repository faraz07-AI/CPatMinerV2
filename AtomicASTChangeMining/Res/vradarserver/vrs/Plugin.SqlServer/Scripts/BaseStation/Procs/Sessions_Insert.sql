IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Insert]
    @LocationID INT
   ,@StartTime  DATETIME2
   ,@EndTime    DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @LocationID = CASE WHEN @LocationID = 0 THEN NULL ELSE @LocationID END;

    INSERT INTO [BaseStation].[Sessions] (
         [LocationID]
        ,[StartTime]
        ,[EndTime]
    ) VALUES (
         @LocationID
        ,@StartTime
        ,@EndTime
    );

    SELECT SCOPE_IDENTITY() AS [SessionID];
END;
GO

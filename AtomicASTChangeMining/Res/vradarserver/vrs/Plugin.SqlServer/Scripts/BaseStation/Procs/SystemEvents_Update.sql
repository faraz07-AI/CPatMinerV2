IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_Update]
    @SystemEventsID INT
   ,@TimeStamp      DATETIME2
   ,@App            NVARCHAR(500)
   ,@Msg            NVARCHAR(2500)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[SystemEvents]
    SET    [TimeStamp]         = @TimeStamp
          ,[App]               = @App
          ,[Msg]               = @Msg
    WHERE [SystemEventsID] = @SystemEventsID;
END;
GO

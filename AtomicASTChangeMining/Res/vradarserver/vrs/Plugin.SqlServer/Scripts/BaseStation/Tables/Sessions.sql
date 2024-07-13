﻿IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Sessions')
BEGIN
    CREATE TABLE [BaseStation].[Sessions]
    (
        [SessionID]     INTEGER IDENTITY
       ,[LocationID]    INTEGER NULL CONSTRAINT [FK_Sessions_Location] FOREIGN KEY REFERENCES [BaseStation].[Locations] ([LocationID])
       ,[StartTime]     DATETIME2 NOT NULL
       ,[EndTime]       DATETIME2 NULL

       ,CONSTRAINT [PK_Sessions] PRIMARY KEY ([SessionID])
    );

    CREATE INDEX [IX_Sessions_EndTime]      ON [BaseStation].[Sessions]([EndTime]);
    CREATE INDEX [IX_Sessions_LocationID]   ON [BaseStation].[Sessions]([LocationID]);
    CREATE INDEX [IX_Sessions_StartTime]    ON [BaseStation].[Sessions]([StartTime]);
END;
GO

-- LocationID was originally not nullable. This causes issues if the location is unknown.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'LocationID' AND IS_NULLABLE = 'YES')
BEGIN
    ALTER TABLE  [BaseStation].[Sessions]
    ALTER COLUMN [LocationID] INTEGER NULL;
END;

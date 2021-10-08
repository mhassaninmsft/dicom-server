/**************************************************************/
--
-- STORED PROCEDURE
--     GetInstancesByWatermarkRangeV2
--
-- FIRST SCHEMA VERSION
--     6
--
-- DESCRIPTION
--     Get instances by given watermark range.
--
-- PARAMETERS
--     @startWatermark
--         * The inclusive start watermark.
--     @endWatermark
--         * The inclusive end watermark.
--     @status
--         * The instance status.
-- RETURN VALUE
--     The instance identifiers.
------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.GetInstancesByWatermarkRangeV2
    @startWatermark BIGINT,
    @endWatermark BIGINT,
    @status TINYINT
AS
BEGIN
    SET NOCOUNT ON
    SET XACT_ABORT ON
    SELECT PartitionName,
           StudyInstanceUid,
           SeriesInstanceUid,
           SopInstanceUid,
           Watermark
    FROM dbo.Instance
    WHERE Watermark BETWEEN @startWatermark AND @endWatermark
          AND Status = @status
END
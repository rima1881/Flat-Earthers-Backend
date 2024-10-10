using LandsatReflectance.Backend.Services;

namespace LandsatReflectance.Backend.Models;

public class Prediction
{
    public int Path { get; set; }
    public int Row { get; set; }
    
    public UsgsDateTimePredictionService.PredictionResults? PredictionResults { get; set; } = new()
    {
        PredictedPublishDate = default,
        AverageTimeSpanBetweenPublishDates = default,
        PredictedPublishDateConfidence = 0,
        PredictedAcquisitionDate = default,
        AverageTimeSpanBetweenAcquisitionDates = default,
        PredictedAcquisitionDateConfidence = 0,
        PredictedSatellite = 0
    };
}
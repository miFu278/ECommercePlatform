using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Product.Domain.ValueObjects;

public class ProductSpecifications
{
    // General specs
    [BsonElement("model")]
    public string? Model { get; set; }

    [BsonElement("weight")]
    public string? Weight { get; set; }

    [BsonElement("dimensions")]
    public ProductDimensions? Dimensions { get; set; }

    [BsonElement("warranty")]
    public string? Warranty { get; set; }

    // CPU specs
    [BsonElement("socket_type")]
    public string? SocketType { get; set; }

    [BsonElement("core_count")]
    public int? CoreCount { get; set; }

    [BsonElement("thread_count")]
    public int? ThreadCount { get; set; }

    [BsonElement("base_clock_ghz")]
    public decimal? BaseClockGhz { get; set; }

    [BsonElement("boost_clock_ghz")]
    public decimal? BoostClockGhz { get; set; }

    [BsonElement("tdp_w")]
    public int? TdpW { get; set; }

    // GPU specs
    [BsonElement("vram_gb")]
    public int? VramGb { get; set; }

    [BsonElement("gpu_chipset")]
    public string? GpuChipset { get; set; }

    // Monitor specs
    [BsonElement("refresh_rate_hz")]
    public int? RefreshRateHz { get; set; }

    [BsonElement("screen_size_inch")]
    public decimal? ScreenSizeInch { get; set; }

    [BsonElement("panel_type")]
    public string? PanelType { get; set; }

    [BsonElement("resolution")]
    public string? Resolution { get; set; }

    // Storage/RAM specs
    [BsonElement("capacity_gb")]
    public int? CapacityGb { get; set; }

    [BsonElement("memory_type")]
    public string? MemoryType { get; set; }

    [BsonElement("memory_speed_mhz")]
    public int? MemorySpeedMhz { get; set; }

    // Mainboard specs
    [BsonElement("chipset")]
    public string? Chipset { get; set; }

    [BsonElement("form_factor")]
    public string? FormFactor { get; set; }

    [BsonElement("memory_slots")]
    public int? MemorySlots { get; set; }

    // PSU specs
    [BsonElement("wattage")]
    public int? Wattage { get; set; }

    [BsonElement("efficiency_rating")]
    public string? EfficiencyRating { get; set; }

    [BsonElement("modular")]
    public string? Modular { get; set; }

    // Case specs
    [BsonElement("case_type")]
    public string? CaseType { get; set; }

    [BsonElement("max_gpu_length_mm")]
    public int? MaxGpuLengthMm { get; set; }

    [BsonElement("max_cooler_height_mm")]
    public int? MaxCoolerHeightMm { get; set; }
}

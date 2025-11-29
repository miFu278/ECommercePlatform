using ECommerce.Product.Domain.Entities;
using ECommerce.Product.Domain.ValueObjects;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Data;

public class CategorySeeder
{
    private readonly IMongoCollection<Category> _collection;
    private readonly Dictionary<string, string> _categoryIds = new();

    public CategorySeeder(IMongoDbContext context)
    {
        _collection = context.GetCollection<Category>("categories");
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        var count = await _collection.CountDocumentsAsync(FilterDefinition<Category>.Empty);
        if (count > 0)
        {
            Console.WriteLine("Categories already seeded. Skipping...");
            return;
        }

        Console.WriteLine("Seeding categories...");

        // Level 0 - Root categories
        await SeedLevel0();
        
        // Level 1 - Sub categories
        await SeedLevel1();
        
        // Level 2 - Sub-sub categories
        await SeedLevel2();

        Console.WriteLine($"Seeded {_categoryIds.Count} categories successfully!");
    }

    private async Task SeedLevel0()
    {
        var categories = new[]
        {
            CreateCategory("PC Build Hoàn Chỉnh", "pc-build-hoan-chinh", "Bộ PC đã build sẵn, cắm điện là chơi", "📦", 1),
            CreateCategory("Linh Kiện Máy Tính", "linh-kien-may-tinh", "Linh kiện PC chính hãng, giá tốt", "🔧", 2),
            CreateCategory("Màn Hình", "man-hinh", "Màn hình máy tính gaming, văn phòng", "🖥️", 3),
            CreateCategory("Gaming Gear", "gaming-gear", "Phụ kiện gaming chuyên nghiệp", "🎮", 4),
            CreateCategory("Loa Máy Tính", "loa-may-tinh", "Loa máy tính 2.0, 2.1, 5.1", "🔊", 5)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 0)");
        }
    }

    private async Task SeedLevel1()
    {
        // PC Build children
        await SeedPCBuildChildren();
        
        // Linh Kiện children
        await SeedLinhKienChildren();
        
        // Màn Hình children
        await SeedManHinhChildren();
        
        // Gaming Gear children
        await SeedGamingGearChildren();
    }

    private async Task SeedPCBuildChildren()
    {
        var parentId = _categoryIds["pc-build-hoan-chinh"];
        var parentPath = new List<string> { parentId };

        var categories = new[]
        {
            CreateCategory("PC Gaming", "pc-gaming", "PC Gaming hiệu năng cao cho game thủ", "🎮", 1, parentId, 1, parentPath),
            CreateCategory("PC Workstation", "pc-workstation", "PC Workstation cho công việc đồ họa, render", "💼", 2, parentId, 1, parentPath),
            CreateCategory("PC Văn Phòng", "pc-van-phong", "PC Văn Phòng giá rẻ, ổn định", "🏢", 3, parentId, 1, parentPath),
            CreateCategory("PC Giả Lập", "pc-gia-lap", "PC tối ưu cho giả lập Android, iOS", "🎬", 4, parentId, 1, parentPath),
            CreateCategory("PC Mini", "pc-mini", "PC Mini gọn nhẹ, tiết kiệm không gian", "📦", 5, parentId, 1, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 1)");
        }
    }

    private async Task SeedLinhKienChildren()
    {
        var parentId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { parentId };

        var categories = new[]
        {
            CreateCategoryWithFilter("CPU - Bộ Vi Xử Lý", "cpu", "CPU Intel, AMD chính hãng", "🧠", 1, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "Intel", "AMD" } },
                    new CategoryFilterMeta { FieldName = "socket_type", DisplayName = "Socket", ValueOptions = new List<string> { "LGA1700", "LGA1200", "AM5", "AM4" } },
                    new CategoryFilterMeta { FieldName = "core_count", DisplayName = "Số nhân", ValueOptions = new List<string> { "4", "6", "8", "12", "16", "24" } }
                }),
            CreateCategoryWithFilter("RAM - Bộ Nhớ", "ram", "RAM DDR4, DDR5 chính hãng", "💾", 2, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "memory_type", DisplayName = "Loại RAM", ValueOptions = new List<string> { "DDR4", "DDR5" } },
                    new CategoryFilterMeta { FieldName = "capacity_gb", DisplayName = "Dung lượng", ValueOptions = new List<string> { "8", "16", "32", "64" } },
                    new CategoryFilterMeta { FieldName = "memory_speed_mhz", DisplayName = "Bus", ValueOptions = new List<string> { "2666", "3200", "3600", "4800", "5200", "6000" } }
                }),
            CreateCategoryWithFilter("SSD - Ổ Cứng", "ssd", "SSD NVMe, SATA tốc độ cao", "💿", 3, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "memory_type", DisplayName = "Loại SSD", ValueOptions = new List<string> { "NVMe M.2", "SATA" } },
                    new CategoryFilterMeta { FieldName = "capacity_gb", DisplayName = "Dung lượng", ValueOptions = new List<string> { "128", "256", "512", "1000", "2000" } }
                }),
            CreateCategoryWithFilter("GPU - Card Đồ Họa", "gpu", "Card đồ họa NVIDIA, AMD", "🎨", 4, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "NVIDIA", "AMD" } },
                    new CategoryFilterMeta { FieldName = "vram_gb", DisplayName = "VRAM", ValueOptions = new List<string> { "4", "6", "8", "12", "16", "24" } },
                    new CategoryFilterMeta { FieldName = "gpu_chipset", DisplayName = "Chipset", ValueOptions = new List<string> { "RTX 4060", "RTX 4070", "RTX 4080", "RX 7600", "RX 7800 XT" } }
                }),
            CreateCategoryWithFilter("Mainboard", "mainboard", "Bo mạch chủ Intel, AMD", "🔌", 5, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "socket_type", DisplayName = "Socket", ValueOptions = new List<string> { "LGA1700", "LGA1200", "AM5", "AM4" } },
                    new CategoryFilterMeta { FieldName = "chipset", DisplayName = "Chipset", ValueOptions = new List<string> { "B760", "Z790", "B650", "X670" } },
                    new CategoryFilterMeta { FieldName = "form_factor", DisplayName = "Form Factor", ValueOptions = new List<string> { "ATX", "mATX", "Mini-ITX" } }
                }),
            CreateCategoryWithFilter("Vỏ Case", "vo-case", "Vỏ case máy tính đẹp, tản nhiệt tốt", "📦", 6, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "form_factor", DisplayName = "Kích thước", ValueOptions = new List<string> { "Full Tower", "Mid Tower", "Mini Tower", "Mini-ITX" } }
                }),
            CreateCategoryWithFilter("Tản Nhiệt", "tan-nhiet", "Tản nhiệt CPU khí, nước", "❄️", 7, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "socket_type", DisplayName = "Socket hỗ trợ", ValueOptions = new List<string> { "LGA1700", "LGA1200", "AM5", "AM4" } }
                }),
            CreateCategoryWithFilter("Nguồn - PSU", "nguon-psu", "Nguồn máy tính 80 Plus", "⚡", 8, parentId, 1, parentPath,
                new[]
                {
                    new CategoryFilterMeta { FieldName = "wattage", DisplayName = "Công suất", ValueOptions = new List<string> { "450", "550", "650", "750", "850", "1000" } },
                    new CategoryFilterMeta { FieldName = "efficiency_rating", DisplayName = "Hiệu suất", ValueOptions = new List<string> { "80 Plus", "80 Plus Bronze", "80 Plus Gold", "80 Plus Platinum" } },
                    new CategoryFilterMeta { FieldName = "modular", DisplayName = "Loại dây", ValueOptions = new List<string> { "Non-Modular", "Semi-Modular", "Full-Modular" } }
                })
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 1)");
        }
    }


    private async Task SeedManHinhChildren()
    {
        var parentId = _categoryIds["man-hinh"];
        var parentPath = new List<string> { parentId };

        var categories = new[]
        {
            CreateCategory("Gaming Monitor", "man-hinh-gaming", "Màn hình gaming tần số cao", "🎮", 1, parentId, 1, parentPath),
            CreateCategory("Văn Phòng", "man-hinh-van-phong", "Màn hình văn phòng giá rẻ", "💼", 2, parentId, 1, parentPath),
            CreateCategory("Đồ Họa", "man-hinh-do-hoa", "Màn hình đồ họa màu chuẩn", "🎨", 3, parentId, 1, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 1)");
        }
    }

    private async Task SeedGamingGearChildren()
    {
        var parentId = _categoryIds["gaming-gear"];
        var parentPath = new List<string> { parentId };

        var categories = new[]
        {
            CreateCategoryWithFilter("Chuột Gaming", "chuot-gaming", "Chuột gaming DPI cao, RGB", "🖱️", 1, parentId, 1, parentPath,
                new[] { new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "Logitech", "Razer", "SteelSeries", "Corsair" } } }),
            CreateCategoryWithFilter("Bàn Phím Gaming", "ban-phim-gaming", "Bàn phím cơ gaming RGB", "⌨️", 2, parentId, 1, parentPath,
                new[] { new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "Logitech", "Razer", "SteelSeries", "Corsair", "Keychron" } } }),
            CreateCategoryWithFilter("Tai Nghe Gaming", "tai-nghe-gaming", "Tai nghe gaming 7.1, RGB", "🎧", 3, parentId, 1, parentPath,
                new[] { new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "Logitech", "Razer", "SteelSeries", "HyperX" } } }),
            CreateCategoryWithFilter("Tay Cầm", "tay-cam", "Tay cầm chơi game Xbox, PS", "🎮", 4, parentId, 1, parentPath,
                new[] { new CategoryFilterMeta { FieldName = "brand_name", DisplayName = "Hãng", ValueOptions = new List<string> { "Xbox", "PlayStation", "Logitech" } } })
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 1)");
        }
    }

    private async Task SeedLevel2()
    {
        // CPU children
        await SeedCPUChildren();
        
        // RAM children
        await SeedRAMChildren();
        
        // SSD children
        await SeedSSDChildren();
        
        // GPU children
        await SeedGPUChildren();
        
        // Mainboard children
        await SeedMainboardChildren();
        
        // Tản Nhiệt children
        await SeedTanNhietChildren();
        
        // Gaming Monitor children
        await SeedGamingMonitorChildren();
    }

    private async Task SeedCPUChildren()
    {
        var parentId = _categoryIds["cpu"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("Intel", "cpu-intel", "CPU Intel Core i3, i5, i7, i9", null, 1, parentId, 2, parentPath),
            CreateCategory("AMD", "cpu-amd", "CPU AMD Ryzen 3, 5, 7, 9", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedRAMChildren()
    {
        var parentId = _categoryIds["ram"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("DDR4", "ram-ddr4", "RAM DDR4 phổ biến, giá tốt", null, 1, parentId, 2, parentPath),
            CreateCategory("DDR5", "ram-ddr5", "RAM DDR5 thế hệ mới, tốc độ cao", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedSSDChildren()
    {
        var parentId = _categoryIds["ssd"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("NVMe M.2", "ssd-nvme", "SSD NVMe M.2 tốc độ siêu nhanh", null, 1, parentId, 2, parentPath),
            CreateCategory("SATA", "ssd-sata", "SSD SATA 2.5 inch giá rẻ", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedGPUChildren()
    {
        var parentId = _categoryIds["gpu"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("NVIDIA", "gpu-nvidia", "Card đồ họa NVIDIA GeForce RTX", null, 1, parentId, 2, parentPath),
            CreateCategory("AMD", "gpu-amd", "Card đồ họa AMD Radeon", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedMainboardChildren()
    {
        var parentId = _categoryIds["mainboard"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("Intel", "mainboard-intel", "Mainboard Intel LGA1700, LGA1200", null, 1, parentId, 2, parentPath),
            CreateCategory("AMD", "mainboard-amd", "Mainboard AMD AM5, AM4", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedTanNhietChildren()
    {
        var parentId = _categoryIds["tan-nhiet"];
        var linhKienId = _categoryIds["linh-kien-may-tinh"];
        var parentPath = new List<string> { linhKienId, parentId };

        var categories = new[]
        {
            CreateCategory("Tản Khí", "tan-khi", "Tản nhiệt khí giá rẻ, hiệu quả", null, 1, parentId, 2, parentPath),
            CreateCategory("Tản Nước", "tan-nuoc", "Tản nhiệt nước AIO hiệu năng cao", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private async Task SeedGamingMonitorChildren()
    {
        var parentId = _categoryIds["man-hinh-gaming"];
        var manHinhId = _categoryIds["man-hinh"];
        var parentPath = new List<string> { manHinhId, parentId };

        var categories = new[]
        {
            CreateCategory("144Hz+", "man-hinh-144hz", "Màn hình 144Hz trở lên", null, 1, parentId, 2, parentPath),
            CreateCategory("4K Gaming", "man-hinh-4k-gaming", "Màn hình 4K cho gaming", null, 2, parentId, 2, parentPath)
        };

        foreach (var category in categories)
        {
            await _collection.InsertOneAsync(category);
            _categoryIds[category.Slug] = category.Id!;
            Console.WriteLine($"  ✓ {category.Name} (Level 2)");
        }
    }

    private Category CreateCategory(string name, string slug, string description, string? icon, int order, 
        string? parentId = null, int level = 0, List<string>? path = null)
    {
        return new Category
        {
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = parentId,
            Level = level,
            Path = path ?? new List<string>(),
            Icon = icon,
            Order = order,
            IsActive = true,
            FilterMeta = new List<CategoryFilterMeta>(),
            Seo = new CategorySeo
            {
                MetaTitle = $"{name} - ECommerce",
                MetaDescription = description
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Category CreateCategoryWithFilter(string name, string slug, string description, string? icon, int order,
        string? parentId, int level, List<string> path, CategoryFilterMeta[] filterMeta)
    {
        var category = CreateCategory(name, slug, description, icon, order, parentId, level, path);
        category.FilterMeta = filterMeta.ToList();
        return category;
    }
}

using ECommerce.Product.Domain.Entities;
using ECommerce.Product.Domain.Enums;
using ECommerce.Product.Domain.ValueObjects;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Data;

public class ProductSeeder
{
    private readonly IMongoCollection<Domain.Entities.Product> _productCollection;
    private readonly IMongoCollection<Category> _categoryCollection;

    public ProductSeeder(IMongoDbContext context)
    {
        _productCollection = context.GetCollection<Domain.Entities.Product>("products");
        _categoryCollection = context.GetCollection<Category>("categories");
    }

    public async Task SeedAsync()
    {
        var count = await _productCollection.CountDocumentsAsync(FilterDefinition<Domain.Entities.Product>.Empty);
        if (count > 0)
        {
            Console.WriteLine("Products already seeded. Skipping...");
            return;
        }

        Console.WriteLine("Seeding products...");

        // Get category IDs
        var cpuIntelCat = await GetCategoryBySlug("cpu-intel");
        var cpuAmdCat = await GetCategoryBySlug("cpu-amd");
        var ramDdr5Cat = await GetCategoryBySlug("ram-ddr5");
        var ssdNvmeCat = await GetCategoryBySlug("ssd-nvme");
        var gpuNvidiaCat = await GetCategoryBySlug("gpu-nvidia");
        var mainboardIntelCat = await GetCategoryBySlug("mainboard-intel");

        // Seed CPUs
        await SeedCPUs(cpuIntelCat!, cpuAmdCat!);
        
        // Seed RAM
        await SeedRAM(ramDdr5Cat!);
        
        // Seed SSD
        await SeedSSD(ssdNvmeCat!);
        
        // Seed GPU
        await SeedGPU(gpuNvidiaCat!);
        
        // Seed Mainboard
        await SeedMainboard(mainboardIntelCat!);

        // Seed PC Builds
        var pcGamingCat = await GetCategoryBySlug("pc-gaming");
        var pcWorkstationCat = await GetCategoryBySlug("pc-workstation");
        var pcVanPhongCat = await GetCategoryBySlug("pc-van-phong");
        var pcGiaLapCat = await GetCategoryBySlug("pc-gia-lap");
        var pcMiniCat = await GetCategoryBySlug("pc-mini");

        if (pcGamingCat != null && pcWorkstationCat != null && pcVanPhongCat != null && 
            pcGiaLapCat != null && pcMiniCat != null)
        {
            await SeedPCBuilds(pcGamingCat, pcWorkstationCat, pcVanPhongCat, pcGiaLapCat, pcMiniCat);
        }
        else
        {
            Console.WriteLine("Warning: Some PC categories not found. Skipping PC builds seed.");
        }

        Console.WriteLine("Products seeded successfully!");
    }

    private async Task SeedCPUs(Category cpuIntelCat, Category cpuAmdCat)
    {
        var products = new[]
        {
            CreateCPU(
                "Intel Core i5-14600K",
                "intel-core-i5-14600k",
                "CPU Intel Core i5-14600K 14 nhân 20 luồng, xung nhịp tối đa 5.3GHz",
                7490000,
                7990000,
                6200000,
                cpuIntelCat,
                "Intel",
                new ProductSpecifications
                {
                    Model = "i5-14600K",
                    SocketType = "LGA1700",
                    CoreCount = 14,
                    ThreadCount = 20,
                    BaseClockGhz = 3.5m,
                    BoostClockGhz = 5.3m,
                    TdpW = 125,
                    Warranty = "36 tháng"
                },
                50,
                "https://example.com/i5-14600k.jpg"
            ),
            CreateCPU(
                "AMD Ryzen 7 7800X3D",
                "amd-ryzen-7-7800x3d",
                "CPU AMD Ryzen 7 7800X3D 8 nhân 16 luồng, công nghệ 3D V-Cache cho gaming",
                9990000,
                10990000,
                8800000,
                cpuAmdCat,
                "AMD",
                new ProductSpecifications
                {
                    Model = "Ryzen 7 7800X3D",
                    SocketType = "AM5",
                    CoreCount = 8,
                    ThreadCount = 16,
                    BaseClockGhz = 4.2m,
                    BoostClockGhz = 5.0m,
                    TdpW = 120,
                    Warranty = "36 tháng"
                },
                30,
                "https://example.com/ryzen-7-7800x3d.jpg"
            ),
            CreateCPU(
                "Intel Core i9-14900K",
                "intel-core-i9-14900k",
                "CPU Intel Core i9-14900K 24 nhân 32 luồng, hiệu năng khủng cho workstation",
                14990000,
                16990000,
                12800000,
                cpuIntelCat,
                "Intel",
                new ProductSpecifications
                {
                    Model = "i9-14900K",
                    SocketType = "LGA1700",
                    CoreCount = 24,
                    ThreadCount = 32,
                    BaseClockGhz = 3.2m,
                    BoostClockGhz = 6.0m,
                    TdpW = 253,
                    Warranty = "36 tháng"
                },
                20,
                "https://example.com/i9-14900k.jpg"
            )
        };

        foreach (var product in products)
        {
            await _productCollection.InsertOneAsync(product);
            Console.WriteLine($"  ✓ {product.Name}");
        }
    }

    private async Task SeedRAM(Category ramCat)
    {
        var products = new[]
        {
            CreateRAM(
                "Corsair Vengeance DDR5 32GB (2x16GB) 6000MHz",
                "corsair-vengeance-ddr5-32gb-6000mhz",
                "RAM Corsair Vengeance DDR5 32GB kit 2x16GB bus 6000MHz RGB",
                3590000,
                3990000,
                2900000,
                ramCat,
                "Corsair",
                new ProductSpecifications
                {
                    Model = "CMK32GX5M2D6000C36",
                    MemoryType = "DDR5",
                    CapacityGb = 32,
                    MemorySpeedMhz = 6000,
                    Warranty = "Lifetime"
                },
                100,
                "https://example.com/corsair-ddr5-32gb.jpg"
            ),
            CreateRAM(
                "G.Skill Trident Z5 RGB DDR5 64GB (2x32GB) 6400MHz",
                "gskill-trident-z5-ddr5-64gb-6400mhz",
                "RAM G.Skill Trident Z5 RGB DDR5 64GB kit 2x32GB bus 6400MHz cho workstation",
                7490000,
                8490000,
                6300000,
                ramCat,
                "G.Skill",
                new ProductSpecifications
                {
                    Model = "F5-6400J3239G32GX2-TZ5RK",
                    MemoryType = "DDR5",
                    CapacityGb = 64,
                    MemorySpeedMhz = 6400,
                    Warranty = "Lifetime"
                },
                50,
                "https://example.com/gskill-ddr5-64gb.jpg"
            )
        };

        foreach (var product in products)
        {
            await _productCollection.InsertOneAsync(product);
            Console.WriteLine($"  ✓ {product.Name}");
        }
    }

    private async Task SeedSSD(Category ssdCat)
    {
        var products = new[]
        {
            CreateSSD(
                "Samsung 990 PRO 1TB NVMe Gen4",
                "samsung-990-pro-1tb",
                "SSD Samsung 990 PRO 1TB NVMe PCIe Gen4 x4 tốc độ đọc 7450MB/s",
                2790000,
                3190000,
                2300000,
                ssdCat,
                "Samsung",
                new ProductSpecifications
                {
                    Model = "MZ-V9P1T0BW",
                    MemoryType = "NVMe M.2",
                    CapacityGb = 1000,
                    Warranty = "60 tháng"
                },
                80,
                "https://example.com/samsung-990-pro-1tb.jpg"
            ),
            CreateSSD(
                "WD Black SN850X 2TB NVMe Gen4",
                "wd-black-sn850x-2tb",
                "SSD WD Black SN850X 2TB NVMe PCIe Gen4 x4 cho gaming",
                4690000,
                5490000,
                3900000,
                ssdCat,
                "Western Digital",
                new ProductSpecifications
                {
                    Model = "WDS200T2X0E",
                    MemoryType = "NVMe M.2",
                    CapacityGb = 2000,
                    Warranty = "60 tháng"
                },
                60,
                "https://example.com/wd-sn850x-2tb.jpg"
            )
        };

        foreach (var product in products)
        {
            await _productCollection.InsertOneAsync(product);
            Console.WriteLine($"  ✓ {product.Name}");
        }
    }


    private async Task SeedGPU(Category gpuCat)
    {
        var products = new[]
        {
            CreateGPU(
                "ASUS ROG Strix GeForce RTX 4070 Ti SUPER OC 16GB",
                "asus-rog-strix-rtx-4070-ti-super-16gb",
                "Card đồ họa ASUS ROG Strix RTX 4070 Ti SUPER 16GB GDDR6X cho gaming 4K",
                23990000,
                26990000,
                20500000,
                gpuCat,
                "ASUS",
                new ProductSpecifications
                {
                    Model = "ROG-STRIX-RTX4070TIS-O16G-GAMING",
                    GpuChipset = "RTX 4070 Ti SUPER",
                    VramGb = 16,
                    Warranty = "36 tháng"
                },
                25,
                "https://example.com/asus-rtx-4070-ti-super.jpg"
            ),
            CreateGPU(
                "MSI GeForce RTX 4060 VENTUS 2X BLACK OC 8GB",
                "msi-rtx-4060-ventus-8gb",
                "Card đồ họa MSI RTX 4060 8GB GDDR6 cho gaming 1080p",
                8490000,
                9490000,
                7200000,
                gpuCat,
                "MSI",
                new ProductSpecifications
                {
                    Model = "RTX 4060 VENTUS 2X BLACK 8G OC",
                    GpuChipset = "RTX 4060",
                    VramGb = 8,
                    Warranty = "36 tháng"
                },
                40,
                "https://example.com/msi-rtx-4060.jpg"
            ),
            CreateGPU(
                "Gigabyte GeForce RTX 4080 SUPER GAMING OC 16GB",
                "gigabyte-rtx-4080-super-16gb",
                "Card đồ họa Gigabyte RTX 4080 SUPER 16GB GDDR6X gaming 4K ultra",
                30990000,
                34990000,
                27000000,
                gpuCat,
                "Gigabyte",
                new ProductSpecifications
                {
                    Model = "GV-N408SGAMING OC-16GD",
                    GpuChipset = "RTX 4080 SUPER",
                    VramGb = 16,
                    Warranty = "36 tháng"
                },
                15,
                "https://example.com/gigabyte-rtx-4080-super.jpg"
            )
        };

        foreach (var product in products)
        {
            await _productCollection.InsertOneAsync(product);
            Console.WriteLine($"  ✓ {product.Name}");
        }
    }

    private async Task SeedMainboard(Category mainboardCat)
    {
        var products = new[]
        {
            CreateMainboard(
                "ASUS ROG STRIX Z790-E GAMING WIFI",
                "asus-rog-strix-z790-e-gaming-wifi",
                "Mainboard ASUS ROG STRIX Z790-E GAMING WIFI ATX hỗ trợ Intel Gen 14",
                11990000,
                13990000,
                10200000,
                mainboardCat,
                "ASUS",
                new ProductSpecifications
                {
                    Model = "ROG STRIX Z790-E GAMING WIFI",
                    SocketType = "LGA1700",
                    Chipset = "Z790",
                    FormFactor = "ATX",
                    MemorySlots = 4,
                    Warranty = "36 tháng"
                },
                30,
                "https://example.com/asus-z790-e.jpg"
            ),
            CreateMainboard(
                "MSI MAG B760 TOMAHAWK WIFI DDR4",
                "msi-mag-b760-tomahawk-wifi-ddr4",
                "Mainboard MSI MAG B760 TOMAHAWK WIFI ATX hỗ trợ Intel Gen 13/14",
                5490000,
                6490000,
                4700000,
                mainboardCat,
                "MSI",
                new ProductSpecifications
                {
                    Model = "MAG B760 TOMAHAWK WIFI DDR4",
                    SocketType = "LGA1700",
                    Chipset = "B760",
                    FormFactor = "ATX",
                    MemorySlots = 4,
                    Warranty = "36 tháng"
                },
                45,
                "https://example.com/msi-b760-tomahawk.jpg"
            )
        };

        foreach (var product in products)
        {
            await _productCollection.InsertOneAsync(product);
            Console.WriteLine($"  ✓ {product.Name}");
        }
    }

    private async Task<Category?> GetCategoryBySlug(string slug)
    {
        return await _categoryCollection.Find(c => c.Slug == slug).FirstOrDefaultAsync();
    }

    private Domain.Entities.Product CreateCPU(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl)
    {
        return CreateProduct(name, slug, description, price, compareAtPrice, costPrice, category, brand, specs, stock, imageUrl, new[] { "CPU", "Gaming", "Workstation" });
    }

    private Domain.Entities.Product CreateRAM(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl)
    {
        return CreateProduct(name, slug, description, price, compareAtPrice, costPrice, category, brand, specs, stock, imageUrl, new[] { "RAM", "Memory", "Gaming" });
    }

    private Domain.Entities.Product CreateSSD(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl)
    {
        return CreateProduct(name, slug, description, price, compareAtPrice, costPrice, category, brand, specs, stock, imageUrl, new[] { "SSD", "Storage", "NVMe" });
    }

    private Domain.Entities.Product CreateGPU(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl)
    {
        return CreateProduct(name, slug, description, price, compareAtPrice, costPrice, category, brand, specs, stock, imageUrl, new[] { "GPU", "Graphics Card", "Gaming", "RTX" });
    }

    private Domain.Entities.Product CreateMainboard(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl)
    {
        return CreateProduct(name, slug, description, price, compareAtPrice, costPrice, category, brand, specs, stock, imageUrl, new[] { "Mainboard", "Motherboard", "Gaming" });
    }

    private async Task SeedPCBuilds(Category pcGaming, Category pcWorkstation, Category pcVanPhong, Category pcGiaLap, Category pcMini)
    {
        // PC Gaming
        var gamingPCs = new[]
        {
            CreatePCBuild("PC Gaming RTX 4060 - Intel i5 14400F", "pc-gaming-rtx-4060-i5-14400f",
                "PC Gaming RTX 4060 8GB + i5-14400F, chơi mượt 1080p Ultra", 18990000, 20990000, 16500000,
                pcGaming, "i5-14400F | RTX 4060 8GB | 16GB DDR4 | SSD 500GB", 15),
            CreatePCBuild("PC Gaming RTX 4070 - Ryzen 7 7800X3D", "pc-gaming-rtx-4070-r7-7800x3d",
                "PC Gaming RTX 4070 12GB + Ryzen 7 7800X3D, gaming 2K đỉnh cao", 29990000, 32990000, 26000000,
                pcGaming, "Ryzen 7 7800X3D | RTX 4070 12GB | 32GB DDR5 | SSD 1TB", 10),
            CreatePCBuild("PC Gaming RTX 4060 Ti - Ryzen 5 7600", "pc-gaming-rtx-4060ti-r5-7600",
                "PC Gaming RTX 4060 Ti 8GB + Ryzen 5 7600, cân mọi game 1080p-1440p", 22990000, 24990000, 19500000,
                pcGaming, "Ryzen 5 7600 | RTX 4060 Ti 8GB | 16GB DDR5 | SSD 500GB", 12),
            CreatePCBuild("PC Gaming RTX 4070 Ti Super - Intel i7 14700K", "pc-gaming-rtx-4070ti-super-i7-14700k",
                "PC Gaming RTX 4070 Ti Super 16GB + i7-14700K, chiến 4K mượt mà", 38990000, 42990000, 34000000,
                pcGaming, "i7-14700K | RTX 4070 Ti Super 16GB | 32GB DDR5 | SSD 2TB", 8),
            CreatePCBuild("PC Gaming RTX 4060 - Ryzen 5 5600", "pc-gaming-rtx-4060-r5-5600",
                "PC Gaming RTX 4060 8GB + Ryzen 5 5600, giá rẻ hiệu năng tốt", 16490000, 17990000, 14000000,
                pcGaming, "Ryzen 5 5600 | RTX 4060 8GB | 16GB DDR4 | SSD 500GB", 18),
        };

        // PC Workstation
        var workstationPCs = new[]
        {
            CreatePCBuild("PC Workstation RTX 4070 - Ryzen 9 7900X", "pc-workstation-rtx-4070-r9-7900x",
                "PC Workstation RTX 4070 + Ryzen 9 7900X, render 3D chuyên nghiệp", 35990000, 38990000, 31000000,
                pcWorkstation, "Ryzen 9 7900X | RTX 4070 12GB | 64GB DDR5 | SSD 2TB", 8),
            CreatePCBuild("PC Workstation RTX 4080 - Intel i9 14900K", "pc-workstation-rtx-4080-i9-14900k",
                "PC Workstation RTX 4080 + i9-14900K, đồ họa 4K chuyên sâu", 48990000, null, 42000000,
                pcWorkstation, "i9-14900K | RTX 4080 16GB | 64GB DDR5 | SSD 4TB", 5),
            CreatePCBuild("PC Workstation RTX 4060 Ti - Ryzen 7 7700X", "pc-workstation-rtx-4060ti-r7-7700x",
                "PC Workstation RTX 4060 Ti + Ryzen 7 7700X, thiết kế đồ họa", 26990000, 28990000, 23000000,
                pcWorkstation, "Ryzen 7 7700X | RTX 4060 Ti 8GB | 32GB DDR5 | SSD 1TB", 10),
            CreatePCBuild("PC Workstation RTX 4070 - Intel i7 14700", "pc-workstation-rtx-4070-i7-14700",
                "PC Workstation RTX 4070 + i7-14700, render video 4K mượt", 32990000, 35990000, 28500000,
                pcWorkstation, "i7-14700 | RTX 4070 12GB | 32GB DDR5 | SSD 2TB", 7),
            CreatePCBuild("PC Workstation RTX 4070 Ti - Ryzen 9 7950X", "pc-workstation-rtx-4070ti-r9-7950x",
                "PC Workstation RTX 4070 Ti + Ryzen 9 7950X, đa nhiệm cực mạnh", 42990000, null, 37000000,
                pcWorkstation, "Ryzen 9 7950X | RTX 4070 Ti 12GB | 64GB DDR5 | SSD 4TB", 6),
        };

        // PC Văn Phòng
        var officePCs = new[]
        {
            CreatePCBuild("PC Văn Phòng Intel i3 12100", "pc-van-phong-i3-12100",
                "PC Văn Phòng i3-12100, xử lý văn bản mượt mà", 8990000, 9990000, 7500000,
                pcVanPhong, "i3-12100 | Intel UHD 730 | 8GB DDR4 | SSD 256GB", 25),
            CreatePCBuild("PC Văn Phòng Ryzen 5 5600G", "pc-van-phong-r5-5600g",
                "PC Văn Phòng Ryzen 5 5600G, đồ họa tích hợp mạnh", 10990000, 11990000, 9000000,
                pcVanPhong, "Ryzen 5 5600G | Radeon Graphics | 16GB DDR4 | SSD 512GB", 20),
            CreatePCBuild("PC Văn Phòng Intel i5 12400", "pc-van-phong-i5-12400",
                "PC Văn Phòng i5-12400, đa nhiệm tốt", 11990000, 12990000, 10000000,
                pcVanPhong, "i5-12400 | Intel UHD 730 | 16GB DDR4 | SSD 512GB", 18),
            CreatePCBuild("PC Văn Phòng Ryzen 3 4100", "pc-van-phong-r3-4100",
                "PC Văn Phòng Ryzen 3 4100, giá rẻ ổn định", 7490000, 8490000, 6200000,
                pcVanPhong, "Ryzen 3 4100 | GT 730 | 8GB DDR4 | SSD 256GB", 30),
            CreatePCBuild("PC Văn Phòng Intel i5 13400", "pc-van-phong-i5-13400",
                "PC Văn Phòng i5-13400, hiệu năng cao", 13490000, 14490000, 11500000,
                pcVanPhong, "i5-13400 | Intel UHD 730 | 16GB DDR4 | SSD 512GB", 15),
        };

        // PC Giả Lập
        var emuPCs = new[]
        {
            CreatePCBuild("PC Giả Lập 8 Luồng - Ryzen 5 5600", "pc-gia-lap-8-luong-r5-5600",
                "PC Giả Lập Ryzen 5 5600, chạy 4-6 giả lập mượt", 12990000, 13990000, 10800000,
                pcGiaLap, "Ryzen 5 5600 | GT 1030 | 32GB DDR4 | SSD 512GB", 15),
            CreatePCBuild("PC Giả Lập 12 Luồng - Ryzen 5 5600X", "pc-gia-lap-12-luong-r5-5600x",
                "PC Giả Lập Ryzen 5 5600X, chạy 6-8 giả lập", 14990000, 15990000, 12500000,
                pcGiaLap, "Ryzen 5 5600X | GTX 1650 | 32GB DDR4 | SSD 512GB", 12),
            CreatePCBuild("PC Giả Lập 16 Luồng - Ryzen 7 5700X", "pc-gia-lap-16-luong-r7-5700x",
                "PC Giả Lập Ryzen 7 5700X, chạy 8-12 giả lập", 17990000, 19990000, 15000000,
                pcGiaLap, "Ryzen 7 5700X | GTX 1650 | 64GB DDR4 | SSD 1TB", 10),
            CreatePCBuild("PC Giả Lập 20 Luồng - Intel i7 12700", "pc-gia-lap-20-luong-i7-12700",
                "PC Giả Lập i7-12700, chạy 12-16 giả lập", 21990000, 23990000, 18500000,
                pcGiaLap, "i7-12700 | GTX 1660 Super | 64GB DDR4 | SSD 1TB", 8),
            CreatePCBuild("PC Giả Lập 24 Luồng - Ryzen 9 5900X", "pc-gia-lap-24-luong-r9-5900x",
                "PC Giả Lập Ryzen 9 5900X, chạy 16-20 giả lập", 24990000, null, 21000000,
                pcGiaLap, "Ryzen 9 5900X | RTX 3060 | 64GB DDR4 | SSD 2TB", 6),
        };

        // PC Mini
        var miniPCs = new[]
        {
            CreatePCBuild("PC Mini Intel i3 12100", "pc-mini-i3-12100",
                "PC Mini i3-12100, gọn nhẹ văn phòng", 9490000, 10490000, 7900000,
                pcMini, "i3-12100 | Intel UHD 730 | 8GB DDR4 | SSD 256GB", 20),
            CreatePCBuild("PC Mini Ryzen 5 5600G", "pc-mini-r5-5600g",
                "PC Mini Ryzen 5 5600G, đồ họa tích hợp tốt", 11990000, 12990000, 10000000,
                pcMini, "Ryzen 5 5600G | Radeon Graphics | 16GB DDR4 | SSD 512GB", 15),
            CreatePCBuild("PC Mini Gaming RTX 4060", "pc-mini-gaming-rtx-4060",
                "PC Mini Gaming RTX 4060, nhỏ gọn mạnh mẽ", 21990000, 23990000, 18500000,
                pcMini, "i5-13400 | RTX 4060 8GB | 16GB DDR5 | SSD 1TB", 10),
            CreatePCBuild("PC Mini Intel i5 13500", "pc-mini-i5-13500",
                "PC Mini i5-13500, hiệu năng cao gọn gàng", 13990000, 14990000, 11800000,
                pcMini, "i5-13500 | Intel UHD 770 | 16GB DDR4 | SSD 512GB", 12),
            CreatePCBuild("PC Mini Ryzen 7 5700G", "pc-mini-r7-5700g",
                "PC Mini Ryzen 7 5700G, đa nhiệm mượt", 14990000, 15990000, 12500000,
                pcMini, "Ryzen 7 5700G | Radeon Graphics | 32GB DDR4 | SSD 1TB", 10),
        };

        foreach (var pc in gamingPCs.Concat(workstationPCs).Concat(officePCs).Concat(emuPCs).Concat(miniPCs))
        {
            await _productCollection.InsertOneAsync(pc);
            Console.WriteLine($"  ✓ {pc.Name}");
        }
    }

    private Domain.Entities.Product CreatePCBuild(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string specs, int stock)
    {
        var sku = $"PC-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var imageUrl = "https://images.unsplash.com/photo-1587202372634-32705e3bf49c?w=600";
        
        return new Domain.Entities.Product
        {
            Sku = sku,
            Name = name,
            Slug = slug,
            Description = description,
            LongDescription = $"{description}. Cấu hình: {specs}. Bảo hành 24 tháng, hỗ trợ kỹ thuật trọn đời.",
            Price = price,
            CompareAtPrice = compareAtPrice,
            CostPrice = costPrice,
            Stock = stock,
            LowStockThreshold = 5,
            TrackInventory = true,
            CategoryId = category.Id!,
            CategoryPath = category.Path,
            BrandName = "TTG Shop",
            Images = new List<ProductImage>
            {
                new() { Url = imageUrl, AltText = name, IsPrimary = true, Order = 1 }
            },
            Attributes = new List<ProductAttribute>(),
            Specifications = new ProductSpecifications
            {
                Model = specs,
                Warranty = "24 tháng"
            },
            Seo = new ProductSeo
            {
                MetaTitle = $"{name} - Build PC Chính Hãng",
                MetaDescription = description,
                MetaKeywords = new List<string> { "PC", "Build PC", category.Name }
            },
            Status = ProductStatus.Active,
            IsFeatured = compareAtPrice.HasValue,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            Tags = new List<string> { "PC Build", category.Name, "Chính hãng" },
            Rating = new ProductRating { Average = 4.7m, Count = Random.Shared.Next(15, 80) },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Domain.Entities.Product CreateProduct(string name, string slug, string description, decimal price,
        decimal? compareAtPrice, decimal costPrice, Category category, string brand,
        ProductSpecifications specs, int stock, string imageUrl, string[] tags)
    {
        var sku = $"SKU-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        
        return new Domain.Entities.Product
        {
            Sku = sku,
            Name = name,
            Slug = slug,
            Description = description,
            LongDescription = $"{description}. Sản phẩm chính hãng, bảo hành {specs.Warranty}.",
            Price = price,
            CompareAtPrice = compareAtPrice,
            CostPrice = costPrice,
            Stock = stock,
            LowStockThreshold = 10,
            TrackInventory = true,
            CategoryId = category.Id!,
            CategoryPath = category.Path,
            BrandName = brand,
            Images = new List<ProductImage>
            {
                new() { Url = imageUrl, AltText = name, IsPrimary = true, Order = 1 }
            },
            Attributes = new List<ProductAttribute>(),
            Specifications = specs,
            Seo = new ProductSeo
            {
                MetaTitle = $"{name} - Chính Hãng Giá Tốt",
                MetaDescription = description,
                MetaKeywords = tags.ToList()
            },
            Status = ProductStatus.Active,
            IsFeatured = compareAtPrice.HasValue,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            Tags = tags.ToList(),
            Rating = new ProductRating { Average = 4.5m, Count = Random.Shared.Next(10, 100) },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

# Entity Configurations

## Cách tự động generate Configuration files

### 1. Dùng AI/Copilot (Nhanh nhất ⚡)
- Paste entity class vào chat
- Prompt: "Generate EF Core IEntityTypeConfiguration for this entity"
- Copy kết quả, chỉnh sửa nếu cần

### 2. Dùng EF Core Power Tools (Visual Studio Extension)
```
1. Install: EF Core Power Tools
2. Right-click project → EF Core Power Tools → Reverse Engineer
3. Chọn tables → Generate
```

### 3. Dùng Code Snippet (Manual nhưng nhanh)
Tạo snippet trong IDE:
```csharp
public class $EntityName$Configuration : IEntityTypeConfiguration<$EntityName$>
{
    public void Configure(EntityTypeBuilder<$EntityName$> builder)
    {
        builder.ToTable("$table_name$");
        builder.HasKey(x => x.Id);
        // TODO: Add properties
    }
}
```

### 4. T4 Template (Advanced)
Tạo file `.tt` để auto-generate từ entities.

## Best Practices

✅ **Nên làm:**
- Đặt tên table theo snake_case (products, product_variants)
- Đặt tên column theo snake_case (product_id, created_at)
- Set MaxLength cho string properties
- Set default values khi cần
- Tạo indexes cho foreign keys và search fields
- Document relationships rõ ràng

❌ **Không nên:**
- Hard-code magic numbers
- Quên set IsRequired cho required fields
- Bỏ qua indexes cho performance
- Config quá phức tạp trong một file

## Convention

- File name: `{EntityName}Configuration.cs`
- Class name: `{EntityName}Configuration`
- Table name: lowercase với underscores
- Column name: lowercase với underscores

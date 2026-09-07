// Decompiled with JetBrains decompiler
// Type: NewJira.Infrastructure.Data.JiraDbContextFactory
// Assembly: NewJira.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6D2144EA-A17E-413C-8558-40342ACF835B
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Infrastructure.dll

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

#nullable enable
namespace NewJira.Infrastructure.Data;

public class JiraDbContextFactory : IDesignTimeDbContextFactory<JiraDbContext>
{
    public JiraDbContext CreateDbContext(string[] args)
    {
        return new JiraDbContext(new DbContextOptionsBuilder<JiraDbContext>().UseSqlServer<JiraDbContext>(new ConfigurationBuilder().SetBasePath(JiraDbContextFactory.FindProjectRoot()).AddJsonFile(Path.Combine("NewJira", "appsettings.json"), false).Build().GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection chưa được cấu hình.")).Options);
    }

    private static string FindProjectRoot()
    {
        for (DirectoryInfo? directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
             directoryInfo != null;
             directoryInfo = directoryInfo.Parent)
        {
            if (File.Exists(Path.Combine(directoryInfo.FullName, "NewJira", "appsettings.json")))
                return directoryInfo.FullName;
        }
        throw new DirectoryNotFoundException("Không tìm thấy NewJira/appsettings.json.");
    }
}

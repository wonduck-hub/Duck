// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ProjectAssistant1.Models.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;


namespace ProjectAssistant1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext()
        {

        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // 공식과 같은 코드
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 닷넷 프레임워크 기반에서 호출되는 코드 영역 .net core에서 필요하지 않을 수 있다.
            // App.config 또는 Web.config의 연결 문자열 사용
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString =
                    ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

    }

    

}

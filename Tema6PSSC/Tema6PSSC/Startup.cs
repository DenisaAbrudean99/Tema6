using Tema6PSSC.Data;
using Tema6PSSC.Data.Repositories;
using Tema6PSSC.Events;
using Tema6PSSC.Events.ServiceBus;
using Tema6PSSC.Domeniu;
using Tema6PSSC.Domeniu.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Tema6PSSC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //aici ne adaugam dependintele(nu mai gestionam noi manual creearea obiectelor)
            //configuram accesul la baza de date
            services.AddDbContext<ContextProduse>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //repository
            services.AddTransient<IRepositoryLinieComanda, RepositoryLinieComanda>();
            services.AddTransient<IRepositoryProduse, RepositoryProduse>();
            //workflow
            services.AddTransient<PlasareProdusWorkFlow>();

            services.AddSingleton<IEventSender, ServiceBusTopicEventSender>();

            services.AddAzureClients(builder =>
            {
                builder.AddServiceBusClient(Configuration.GetConnectionString("ServiceBus"));
            });
            //pt a vedea api mai usor
            services.AddControllers();
            //standard
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tema6PSSC", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {//cum facem autorizarea, routarea, ce end pointuri folosim
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tema6PSSC v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

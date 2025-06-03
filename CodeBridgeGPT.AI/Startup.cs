using CodeBridgeGPT.AI.Interfaces;
using CodeBridgeGPT.AI.Services;

namespace CodeBridgeGPT.AI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            services.AddControllers().AddNewtonsoftJson();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpClient();
            services.AddSingleton<IKernelService, CodeBridgeGPTService>();
            services.AddSingleton<IGitHubProcessor, GitHubProcessorService>();
            services.AddSingleton<IGitCommitProcessor, GitCommitProcessor>();
            services.AddSingleton<ICreateRepository, CreateRepositoryService>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            // Enable Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeBridgeGPT API V1"));
        }
    }
}

namespace YourSolution.ConsoleApp
{
    public class Program
    {
        private const string MemberExclusionKey = "Ignored_Member";
        static IConfiguration _configuration;

        /// <summary>
        /// This project is used solely for testing.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            /*
            * Make sure you set up the services in align with your solution's architecture
            * Make sure whereever you use DbFunctions in the map, you have a parallel ResolveUsing 
            * Change the TSource in Main to your source class
            * Change the TDestination in Main to your destination class
            */

            ServiceLocator.Instance
                .ConfigureAppConfiguration(ConfigurationManager.AppSettings["KeyVaultUri"], ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices);

            _configuration = ServiceLocator.Instance.Configuration;

            using (UnitOfWork uow = new UnitOfWork())
            using (IServiceScope serviceScope = ServiceLocator.Instance.ServiceProvider.CreateScope())
            {
                IMapper mapper = serviceScope.ServiceProvider.GetService<IMapper>();
                IQueryable<TSource> source = uow.YOUR_SOURCE_REPO.GetAll().OrderByDescending(r => r.ID).Take(100);

                PropertyMap[] propertyMapsNotIgnored = GetNotIgnoredPropertyMaps<TSource, TDestination>(mapper);
                if (propertyMapsNotIgnored == null)
                    return;

                //Complete mapping - Doing twice b/c 1st would be cold
                Map<TSource, TDestination>(source, mapper, null); 
                Map<TSource, TDestination>(source, mapper, null); 

                //Partial mapping
                Console.WriteLine("");
                Console.WriteLine("========Start Mapping with Individual Property ignored========");

                for (int i = 0; i < propertyMapsNotIgnored.Length; i++)
                    Map<TSource, TDestination>(source, mapper, propertyMapsNotIgnored[i]);
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        private static PropertyMap[] GetNotIgnoredPropertyMaps<TSource, TDestination>(IMapper mapper)
        {
            TypeMap typeMap = mapper.ConfigurationProvider.FindTypeMapFor<TSource, TDestination>();
            if (typeMap == null)
            {
                Console.WriteLine($"Mapping config from {nameof(TSource)} to {nameof(TDestination)} does not exisist.");
                return null;
            }

            PropertyMap[] propertyMaps = typeMap.GetPropertyMaps();
            PropertyMap[] propertyMapsNotIgnored = propertyMaps.Where(pm => !pm.Ignored).ToArray();
            return propertyMapsNotIgnored;
        }

        private static void Map<TSource, TDestination>(IQueryable<TSource> source, IMapper mapper, PropertyMap pm)
        {
            try
            {
                List<TSource> sourceList = source.ToList();
                TDestination destinationItem;
                string ignoredMember = pm?.DestinationProperty.Name ?? string.Empty;

                DateTime start = DateTime.UtcNow;
                foreach (TSource sourceItem in sourceList)
                    destinationItem = mapper.Map<TDestination>(sourceItem, opts => opts.ExcludeMember(ignoredMember));
                DateTime end = DateTime.UtcNow;

                if (string.IsNullOrEmpty(ignoredMember))
                    Console.WriteLine($"Complete Mapping: {(end - start).TotalMilliseconds} ms");
                else
                    Console.WriteLine($"Mapping with {ignoredMember} ignored: {(end - start).TotalMilliseconds} ms");
            }
            catch (Exception ex)
            {
                if (pm == null)
                    Console.WriteLine("Complete mapping threw exception: " + ex.Message);
                else
                    Console.WriteLine($"Ignoring {pm.DestinationProperty.Name} threw exception: " + ex.Message);
            }
        }

        private static void ConfigureAppConfiguration(IConfigurationBuilder configBuilder)
        {
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAutoMapper(cfg =>
                {
                    cfg.ForAllMaps((tm, me) => tm.MaxDepth = 1);
                    cfg.ForAllMaps((tm, me) =>
                    {
                        me.ForAllMembers(memberOptions =>
                        {
                            memberOptions.Condition((o1, o2, o3, o4, resolutionContext) =>
                            {
                                string name = memberOptions.DestinationMember.Name;
                                if (resolutionContext.Items.ContainsKey(MemberExclusionKey) && (string)resolutionContext.Items[MemberExclusionKey] == name)
                                    return false;
                                return true;
                            });
                        });
                    });
                }, typeof(ViewModelsProfile)) //Replace with your mapping profile
                .AddEntityFramework();
                //Add other necessary services
        }
    }
}
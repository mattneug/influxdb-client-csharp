using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Internal;

namespace InfluxDB.Client
{
    public interface IInfluxDBClient : IDisposable
    {
        /// <summary>
        /// Get the Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new client instance for the Query API</returns>
        IQueryApi GetQueryApi(IDomainObjectMapper mapper = null);

        /// <summary>
        /// Get the synchronous version of Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new synchronous client instance for the Query API</returns>
        IQueryApiSync GetQueryApiSync(IDomainObjectMapper mapper = null);

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        IWriteApi GetWriteApi(IDomainObjectMapper mapper = null);

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        IWriteApi GetWriteApi(WriteOptions writeOptions, IDomainObjectMapper mapper = null);

        /// <summary>
        /// Get the Write async client.
        /// </summary>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API Async without batching</returns>
        IWriteApiAsync GetWriteApiAsync(IDomainObjectMapper mapper = null);

        /// <summary>
        /// Get the <see cref="Organization" /> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        IOrganizationsApi GetOrganizationsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.User" /> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        IUsersApi GetUsersApi();

        /// <summary>
        /// Get the <see cref="Bucket" /> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        IBucketsApi GetBucketsApi();

        /// <summary>
        /// Get the <see cref="Source" /> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        ISourcesApi GetSourcesApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Authorization" /> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        IAuthorizationsApi GetAuthorizationsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.TaskType" /> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        ITasksApi GetTasksApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.ScraperTargetResponse" /> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        IScraperTargetsApi GetScraperTargetsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Telegraf" /> client.
        /// </summary>
        /// <returns>the new client instance for Telegrafs API</returns>
        ITelegrafsApi GetTelegrafsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Label" /> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        ILabelsApi GetLabelsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationEndpoint" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationEndpoint API</returns>
        INotificationEndpointsApi GetNotificationEndpointsApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationRules" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationRules API</returns>
        INotificationRulesApi GetNotificationRulesApi();

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Check" /> client.
        /// </summary>
        /// <returns>the new client instance for Checks API</returns>
        IChecksApi GetChecksApi();

        /// <summary>
        /// Get the Delete client.
        /// </summary>
        /// <returns>the new client instance for Delete API</returns>
        IDeleteApi GetDeleteApi();

        /// <summary>
        /// Create an InvokableScripts API instance.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping invocation results to POCO</param>
        /// <returns>New instance of InvokableScriptsApi.</returns>
        IInvokableScriptsApi GetInvokableScriptsApi(IDomainObjectMapper mapper = null);

        /// <summary>
        /// Create a service for specified type.
        /// </summary>
        /// <param name="serviceType">type of service</param>
        /// <typeparam name="TS">type of service</typeparam>
        /// <returns>new instance of service</returns>
        TS CreateService<TS>(Type serviceType) where TS : IApiAccessor;

        /// <summary>
        /// Set the log level for the request and response information.
        /// </summary>
        /// <param name="logLevel">the log level to set</param>
        void SetLogLevel(LogLevel logLevel);

        /// <summary>
        /// Set the <see cref="LogLevel" /> that is used for logging requests and responses.
        /// </summary>
        /// <returns>Log Level</returns>
        LogLevel GetLogLevel();

        /// <summary>
        /// Enable Gzip compress for http requests.
        ///
        /// <para>Currently only the "Write" and "Query" endpoints supports the Gzip compression.</para>
        /// </summary>
        /// <returns></returns>
        IInfluxDBClient EnableGzip();

        /// <summary>
        /// Disable Gzip compress for http request body.
        /// </summary>
        /// <returns>this</returns>
        IInfluxDBClient DisableGzip();

        /// <summary>
        /// Returns whether Gzip compress for http request body is enabled.
        /// </summary>
        /// <returns>true if gzip is enabled.</returns>
        bool IsGzipEnabled();

        /// <summary>
        /// Get the health of an instance.
        /// </summary>
        /// <returns>health of an instance</returns>
        [Obsolete("This method is obsolete. Call 'PingAsync()' instead.", false)]
        Task<HealthCheck> HealthAsync();

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <returns>true if server is healthy otherwise return false</returns>
        Task<bool> PingAsync();

        /// <summary>
        ///  Return the version of the connected InfluxDB Server.
        /// </summary>
        /// <returns>the version String, otherwise unknown</returns>
        /// <exception cref="InfluxException">throws when request did not succesfully ends</exception>
        Task<string> VersionAsync();

        /// <summary>
        /// Check the readiness of InfluxDB Server at startup. It is not supported by InfluxDB Cloud. 
        /// </summary>
        /// <returns>return null if the InfluxDB is not ready</returns>
        Task<Ready> ReadyAsync();

        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="onboarding">to setup defaults</param>
        /// <exception cref="HttpException">With status code 422 when an onboarding has already been completed</exception>
        /// <returns>defaults for first run</returns>
        Task<OnboardingResponse> OnboardingAsync(OnboardingRequest onboarding);

        /// <summary>
        /// Check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <returns>True if onboarding has already been completed otherwise false</returns>
        Task<bool> IsOnboardingAllowedAsync();
    }

    public class InfluxDBClient : AbstractRestClient, IInfluxDBClient
    {
        private readonly ApiClient _apiClient;
        private readonly ExceptionFactory _exceptionFactory;
        private readonly HealthService _healthService;
        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;
        private readonly ReadyService _readyService;
        private readonly PingService _pingService;

        private readonly SetupService _setupService;
        private readonly InfluxDBClientOptions _options;

        private readonly Subject<Unit> _disposeNotification = new Subject<Unit>();

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client. The url could be a connection string with various configurations.
        /// <para>
        /// e.g.: "http://localhost:8086?timeout=5000&amp;logLevel=BASIC
        /// The following options are supported:
        /// <list type="bullet">
        /// <item>org - default destination organization for writes and queries</item>
        /// <item>bucket - default destination bucket for writes</item>
        /// <item>token - the token to use for the authorization</item>
        /// <item>logLevel (default - NONE) - rest client verbosity level</item>
        /// <item>timeout (default - 10000) - The timespan to wait before the HTTP request times out in milliseconds</item>
        /// <item>allowHttpRedirects (default - false) - Configure automatically following HTTP 3xx redirects</item>
        /// <item>verifySsl (default - true) - Ignore Certificate Validation Errors when false</item>
        /// </list>
        /// Options for logLevel:
        /// <list type="bullet">
        /// <item>Basic - Logs request and response lines.</item>
        /// <item>Body - Logs request and response lines including headers and body (if present). Note that applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.</item>
        /// <item>Headers - Logs request and response lines including headers.</item>
        /// <item>None - Disable logging.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="url">connection string with various configurations</param>
        public InfluxDBClient(string url) :
            this(new InfluxDBClientOptions(url))
        {
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.x</param>
        /// <param name="username">the username to use in the basic auth</param>
        /// <param name="password">the password to use in the basic auth</param>
        public InfluxDBClient(string url, string username, string password) :
            this(new InfluxDBClientOptions(url)
            {
                Username = username,
                Password = password
            })
        {
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.x</param>
        /// <param name="token">the token to use for the authorization</param>
        public InfluxDBClient(string url, string token) :
            this(new InfluxDBClientOptions(url)
            {
                Token = token
            })
        {
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client to connect into InfluxDB 1.8.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 1.8</param>
        /// <param name="username">authorization username</param>
        /// <param name="password">authorization password</param>
        /// <param name="database">database name</param>
        /// <param name="retentionPolicy">retention policy</param>
        public InfluxDBClient(string url, string username, string password, string database, string retentionPolicy) :
            this(new InfluxDBClientOptions(url)
            {
                Org = "-",
                Token = $"{username}:{password}",
                Bucket = $"{database}/{retentionPolicy}"
            })
        {
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="options">the connection configuration</param>
        public InfluxDBClient(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, nameof(options));

            _options = options;
            _loggingHandler = new LoggingHandler(options.LogLevel);
            _gzipHandler = new GzipHandler();

            _apiClient = new ApiClient(options, _loggingHandler, _gzipHandler);

            _exceptionFactory = (methodName, response) =>
                !response.IsSuccessful ? HttpException.Create(response, response.Content) : null;

            _setupService = new SetupService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            _healthService = new HealthService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            _readyService = new ReadyService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            _pingService = new PingService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
        }

        public void Dispose()
        {
            //
            // Dispose child APIs
            //
            _disposeNotification.OnNext(Unit.Default);

            //
            // signout
            //
            try
            {
                _apiClient.Signout();
            }
            catch (Exception e)
            {
                Trace.WriteLine("The signout exception");
                Trace.WriteLine(e);
            }

            // 
            // Dispose HttpClient 
            // 
            _apiClient.RestClient.Dispose();
        }

        public DashboardApi GetDashboardApi()
        {
            var service = new DashboardsService((Configuration)_apiClient.Configuration) 
            { 
                ExceptionFactory = _exceptionFactory 
            };

            return new DashboardApi(_options, service);
        }

        /// <summary>
        /// Get the Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new client instance for the Query API</returns>
        IQueryApi IInfluxDBClient.GetQueryApi(IDomainObjectMapper mapper)
        {
            return GetQueryApi(mapper);
        }

        /// <summary>
        /// Get the Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new client instance for the Query API</returns>
        public QueryApi GetQueryApi(IDomainObjectMapper mapper = null)
        {
            var service = new QueryService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new QueryApi(_options, service, mapper ?? new DefaultDomainObjectMapper());
        }

        /// <summary>
        /// Get the synchronous version of Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new synchronous client instance for the Query API</returns>
        IQueryApiSync IInfluxDBClient.GetQueryApiSync(IDomainObjectMapper mapper)
        {
            return GetQueryApiSync(mapper);
        }

        /// <summary>
        /// Get the synchronous version of Query client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping FluxResults to POCO</param>
        /// <returns>the new synchronous client instance for the Query API</returns>
        public QueryApiSync GetQueryApiSync(IDomainObjectMapper mapper = null)
        {
            var service = new QueryService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new QueryApiSync(_options, service, mapper ?? new DefaultDomainObjectMapper());
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        IWriteApi IInfluxDBClient.GetWriteApi(IDomainObjectMapper mapper)
        {
            return GetWriteApi(mapper);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi(IDomainObjectMapper mapper = null)
        {
            return GetWriteApi(new WriteOptions(), mapper);
        }

        /// <summary>
        /// Get the Write async client.
        /// </summary>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API Async without batching</returns>
        IWriteApiAsync IInfluxDBClient.GetWriteApiAsync(IDomainObjectMapper mapper)
        {
            return GetWriteApiAsync(mapper);
        }

        /// <summary>
        /// Get the Write async client.
        /// </summary>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API Async without batching</returns>
        public WriteApiAsync GetWriteApiAsync(IDomainObjectMapper mapper = null)
        {
            var service = new WriteService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new WriteApiAsync(_options, service, mapper ?? new DefaultDomainObjectMapper(), this);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        IWriteApi IInfluxDBClient.GetWriteApi(WriteOptions writeOptions, IDomainObjectMapper mapper)
        {
            return GetWriteApi(writeOptions, mapper);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <param name="mapper">the converter used for mapping to PointData</param>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi(WriteOptions writeOptions, IDomainObjectMapper mapper = null)
        {
            var service = new WriteService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            var writeApi = new WriteApi(_options, service, writeOptions, mapper ?? new DefaultDomainObjectMapper(),
                this, _disposeNotification);

            return writeApi;
        }

        /// <summary>
        /// Get the <see cref="Organization" /> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        IOrganizationsApi IInfluxDBClient.GetOrganizationsApi()
        {
            return GetOrganizationsApi();
        }

        /// <summary>
        /// Get the <see cref="Organization" /> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        public OrganizationsApi GetOrganizationsApi()
        {
            var service = new OrganizationsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            var secretService = new SecretsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new OrganizationsApi(service, secretService);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.User" /> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        IUsersApi IInfluxDBClient.GetUsersApi()
        {
            return GetUsersApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.User" /> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        public UsersApi GetUsersApi()
        {
            var service = new UsersService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new UsersApi(service);
        }

        /// <summary>
        /// Get the <see cref="Bucket" /> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        IBucketsApi IInfluxDBClient.GetBucketsApi()
        {
            return GetBucketsApi();
        }

        /// <summary>
        /// Get the <see cref="Bucket" /> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        public BucketsApi GetBucketsApi()
        {
            var service = new BucketsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new BucketsApi(service);
        }

        /// <summary>
        /// Get the <see cref="Source" /> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        ISourcesApi IInfluxDBClient.GetSourcesApi()
        {
            return GetSourcesApi();
        }

        /// <summary>
        /// Get the <see cref="Source" /> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        public SourcesApi GetSourcesApi()
        {
            var service = new SourcesService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new SourcesApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Authorization" /> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        IAuthorizationsApi IInfluxDBClient.GetAuthorizationsApi()
        {
            return GetAuthorizationsApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Authorization" /> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        public AuthorizationsApi GetAuthorizationsApi()
        {
            var service = new AuthorizationsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new AuthorizationsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.TaskType" /> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        ITasksApi IInfluxDBClient.GetTasksApi()
        {
            return GetTasksApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.TaskType" /> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        public TasksApi GetTasksApi()
        {
            var service = new TasksService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new TasksApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.ScraperTargetResponse" /> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        IScraperTargetsApi IInfluxDBClient.GetScraperTargetsApi()
        {
            return GetScraperTargetsApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.ScraperTargetResponse" /> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        public ScraperTargetsApi GetScraperTargetsApi()
        {
            var service = new ScraperTargetsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new ScraperTargetsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Telegraf" /> client.
        /// </summary>
        /// <returns>the new client instance for Telegrafs API</returns>
        ITelegrafsApi IInfluxDBClient.GetTelegrafsApi()
        {
            return GetTelegrafsApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Telegraf" /> client.
        /// </summary>
        /// <returns>the new client instance for Telegrafs API</returns>
        public TelegrafsApi GetTelegrafsApi()
        {
            var service = new TelegrafsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new TelegrafsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Label" /> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        ILabelsApi IInfluxDBClient.GetLabelsApi()
        {
            return GetLabelsApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Label" /> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        public LabelsApi GetLabelsApi()
        {
            var service = new LabelsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new LabelsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationEndpoint" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationEndpoint API</returns>
        INotificationEndpointsApi IInfluxDBClient.GetNotificationEndpointsApi()
        {
            return GetNotificationEndpointsApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationEndpoint" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationEndpoint API</returns>
        public NotificationEndpointsApi GetNotificationEndpointsApi()
        {
            var service = new NotificationEndpointsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new NotificationEndpointsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationRules" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationRules API</returns>
        INotificationRulesApi IInfluxDBClient.GetNotificationRulesApi()
        {
            return GetNotificationRulesApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationRules" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationRules API</returns>
        public NotificationRulesApi GetNotificationRulesApi()
        {
            var service = new NotificationRulesService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new NotificationRulesApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Check" /> client.
        /// </summary>
        /// <returns>the new client instance for Checks API</returns>
        IChecksApi IInfluxDBClient.GetChecksApi()
        {
            return GetChecksApi();
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Check" /> client.
        /// </summary>
        /// <returns>the new client instance for Checks API</returns>
        public ChecksApi GetChecksApi()
        {
            var service = new ChecksService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new ChecksApi(service);
        }

        /// <summary>
        /// Get the Delete client.
        /// </summary>
        /// <returns>the new client instance for Delete API</returns>
        IDeleteApi IInfluxDBClient.GetDeleteApi()
        {
            return GetDeleteApi();
        }

        /// <summary>
        /// Get the Delete client.
        /// </summary>
        /// <returns>the new client instance for Delete API</returns>
        public DeleteApi GetDeleteApi()
        {
            var service = new DeleteService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new DeleteApi(service);
        }

        /// <summary>
        /// Create an InvokableScripts API instance.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping invocation results to POCO</param>
        /// <returns>New instance of InvokableScriptsApi.</returns>
        IInvokableScriptsApi IInfluxDBClient.GetInvokableScriptsApi(IDomainObjectMapper mapper)
        {
            return GetInvokableScriptsApi(mapper);
        }

        /// <summary>
        /// Create an InvokableScripts API instance.
        /// </summary>
        /// <param name="mapper">the mapper used for mapping invocation results to POCO</param>
        /// <returns>New instance of InvokableScriptsApi.</returns>
        public InvokableScriptsApi GetInvokableScriptsApi(IDomainObjectMapper mapper = null)
        {
            var service = new InvokableScriptsService((Configuration)_apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new InvokableScriptsApi(service, mapper ?? new DefaultDomainObjectMapper());
        }

        /// <summary>
        /// Create a service for specified type.
        /// </summary>
        /// <param name="serviceType">type of service</param>
        /// <typeparam name="TS">type of service</typeparam>
        /// <returns>new instance of service</returns>
        public TS CreateService<TS>(Type serviceType) where TS : IApiAccessor
        {
            var instance = (TS)Activator.CreateInstance(serviceType, (Configuration)_apiClient.Configuration);
            instance.ExceptionFactory = _exceptionFactory;

            return instance;
        }

        /// <summary>
        /// Set the log level for the request and response information.
        /// </summary>
        /// <param name="logLevel">the log level to set</param>
        public void SetLogLevel(LogLevel logLevel)
        {
            Arguments.CheckNotNull(logLevel, nameof(logLevel));

            _loggingHandler.Level = logLevel;
        }

        /// <summary>
        /// Set the <see cref="LogLevel" /> that is used for logging requests and responses.
        /// </summary>
        /// <returns>Log Level</returns>
        public LogLevel GetLogLevel()
        {
            return _loggingHandler.Level;
        }

        /// <summary>
        /// Enable Gzip compress for http requests.
        ///
        /// <para>Currently only the "Write" and "Query" endpoints supports the Gzip compression.</para>
        /// </summary>
        /// <returns></returns>
        IInfluxDBClient IInfluxDBClient.EnableGzip()
        {
            return EnableGzip();
        }

        /// <summary>
        /// Enable Gzip compress for http requests.
        ///
        /// <para>Currently only the "Write" and "Query" endpoints supports the Gzip compression.</para>
        /// </summary>
        /// <returns></returns>
        public InfluxDBClient EnableGzip()
        {
            _gzipHandler.EnableGzip();

            return this;
        }

        /// <summary>
        /// Disable Gzip compress for http request body.
        /// </summary>
        /// <returns>this</returns>
        IInfluxDBClient IInfluxDBClient.DisableGzip()
        {
            return DisableGzip();
        }

        /// <summary>
        /// Disable Gzip compress for http request body.
        /// </summary>
        /// <returns>this</returns>
        public InfluxDBClient DisableGzip()
        {
            _gzipHandler.DisableGzip();

            return this;
        }

        /// <summary>
        /// Returns whether Gzip compress for http request body is enabled.
        /// </summary>
        /// <returns>true if gzip is enabled.</returns>
        public bool IsGzipEnabled()
        {
            return _gzipHandler.IsEnabledGzip();
        }

        /// <summary>
        /// Get the health of an instance.
        /// </summary>
        /// <returns>health of an instance</returns>
        [Obsolete("This method is obsolete. Call 'PingAsync()' instead.", false)]
        public Task<HealthCheck> HealthAsync()
        {
            return GetHealthAsync(_healthService.GetHealthAsync());
        }

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <returns>true if server is healthy otherwise return false</returns>
        public async Task<bool> PingAsync()
        {
            return await PingAsync(_pingService.GetPingAsyncWithIRestResponse());
        }

        /// <summary>
        ///  Return the version of the connected InfluxDB Server.
        /// </summary>
        /// <returns>the version String, otherwise unknown</returns>
        /// <exception cref="InfluxException">throws when request did not succesfully ends</exception>
        public async Task<string> VersionAsync()
        {
            return await VersionAsync(_pingService.GetPingAsyncWithIRestResponse());
        }

        /// <summary>
        /// Check the readiness of InfluxDB Server at startup. It is not supported by InfluxDB Cloud. 
        /// </summary>
        /// <returns>return null if the InfluxDB is not ready</returns>
        public async Task<Ready> ReadyAsync()
        {
            try
            {
                return await _readyService.GetReadyAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Trace.TraceError($"The exception: '{e.Message}' occurs during check instance readiness.");

                return null;
            }
        }

        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="onboarding">to setup defaults</param>
        /// <exception cref="HttpException">With status code 422 when an onboarding has already been completed</exception>
        /// <returns>defaults for first run</returns>
        public Task<OnboardingResponse> OnboardingAsync(OnboardingRequest onboarding)
        {
            Arguments.CheckNotNull(onboarding, nameof(onboarding));

            return _setupService.PostSetupAsync(onboarding);
        }

        /// <summary>
        /// Check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <returns>True if onboarding has already been completed otherwise false</returns>
        public async Task<bool> IsOnboardingAllowedAsync()
        {
            var isOnboarding = await _setupService.GetSetupAsync().ConfigureAwait(false);

            return isOnboarding.Allowed == true;
        }

        internal static string AuthorizationHeader(string username, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));
        }

        internal static async Task<HealthCheck> GetHealthAsync(Task<HealthCheck> task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return new HealthCheck("influxdb", e.Message, default, HealthCheck.StatusEnum.Fail);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using NodaTime;
using NUnit.Framework;
using Duration = NodaTime.Duration;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItWriteQueryApiTest : AbstractItClientTest
    {
        private Bucket _bucket;
        private QueryApi _queryApi;
        private WriteApi _writeApi;
        private Organization _organization;
        private string _token;

        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            _bucket = await Client.GetBucketsApi()
                .CreateBucket(GenerateName("h2o"), retention, _organization);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource =
                new PermissionResource(PermissionResource.TypeEnum.Buckets, _bucket.Id, null, _organization.Id);

            var readBucket = new Permission(Permission.ActionEnum.Read, resource);
            var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

            var loggedUser = await Client.GetUsersApi().Me();
            Assert.IsNotNull(loggedUser);

            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorization(await FindMyOrg(), new List<Permission> {readBucket, writeBucket});

            _token = authorization.Token;

            Client.Dispose();
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl).AuthenticateToken(_token.ToCharArray())
                .Org(_organization.Id).Bucket(_bucket.Id).Build();
            Client = InfluxDBClientFactory.Create(options);
            _queryApi = Client.GetQueryApi();
        }

        [TearDown]
        protected new void After()
        {
            _writeApi.Dispose();
        }

        [Test]
        public async Task WriteRecordsList()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            _writeApi.WriteRecords(bucketName, _organization.Id, WritePrecision.Ns,
                new List<string> {record1, record2});
            _writeApi.Flush();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task WriteRecordsParams()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteRecords(bucketName, _organization.Id, WritePrecision.Ns, record1, record2);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task WritePoints()
        {
            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var point1 = Point
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, WritePrecision.S);

            var point2 = Point
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(bucketName, _organization.Id, point1, point2);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(1, records[1].GetValue());
            Assert.AreEqual("water_level", records[1].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(10)),
                records[0].GetTimeInDateTime());
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), records[1].GetTimeInDateTime());
        }

        [Test]
        public async Task WritePointsWithoutFields()
        {        
            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var point1 = Point
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Timestamp(time, WritePrecision.S);

            var point2 = Point
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(bucketName, _organization.Id, point1, point2);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 0)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(1, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(TimeSpan.FromSeconds(-10)),
                records[0].GetTimeInDateTime());
        }

        [Test]
        public async Task WriteMeasurements()
        {
            _writeApi = Client.GetWriteApi();

            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = 1.927, Time = time
            };

            _writeApi.WriteMeasurements(bucketName, _organization.Id, WritePrecision.S, measurement1, measurement2);
            _writeApi.Flush();

            var measurements = await _queryApi.Query<H20Measurement>(
                "from(bucket:\"" + bucketName +
                "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> rename(columns:{_value: \"level\"})",
                _organization.Id);

            Assert.AreEqual(2, measurements.Count);

            Assert.AreEqual(2.927, measurements[0].Level);
            Assert.AreEqual("coyote_creek", measurements[0].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(30)),
                measurements[0].Time);

            Assert.AreEqual(1.927, measurements[1].Level);
            Assert.AreEqual("coyote_creek", measurements[1].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), measurements[1].Time);
        }

        [Test]
        public async Task WriteMeasurementsWithoutFields()
        {
            _writeApi = Client.GetWriteApi();

            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = null, Time = time
            };

            _writeApi.WriteMeasurements(bucketName, _organization.Id, WritePrecision.S, measurement1, measurement2);
            _writeApi.Flush();

            var measurements = await _queryApi.Query<H20Measurement>(
                "from(bucket:\"" + bucketName +
                "\") |> range(start: 0) |> rename(columns:{_value: \"level\"})",
                _organization.Id);

            Assert.AreEqual(1, measurements.Count);

            Assert.AreEqual(2.927, measurements[0].Level);
            Assert.AreEqual("coyote_creek", measurements[0].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(30)),
                measurements[0].Time);
        }

        [Test]
        public async Task Flush()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(100_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record);
            _writeApi.Flush();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            var instant = Instant.Add(Instant.FromUnixTimeMilliseconds(0), Duration.FromNanoseconds(1L));
            Assert.AreEqual(instant, records[0].GetTime());
        }

        [Test]
        public async Task FlushByTime()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(500).Build();

            _writeApi = Client.GetWriteApi(writeOptions);
            var listener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record1);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record2);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record3);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record4);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record5);

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            listener.Get<WriteSuccessEvent>();

            query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
        }

        [Test]
        public async Task FlushByCount()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(6).FlushInterval(500_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var listener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";
            const string record6 = "h2o_feet,location=coyote_creek level\\ water_level=6.0 6";

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record1);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record2);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record3);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record4);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record5);

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record6);

            listener.Get<WriteSuccessEvent>();

            query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(6, records.Count);
        }

        [Test]
        public async Task FlushByOne()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).FlushInterval(500_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var eventListener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record1);
            Thread.Sleep(100);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record2);
            Thread.Sleep(100);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record3);
            Thread.Sleep(100);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record4);
            Thread.Sleep(100);
            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record5);
            Thread.Sleep(100);

            Assert.AreEqual(record1, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record2, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record3, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record4, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record5, eventListener.Get<WriteSuccessEvent>().LineProtocol);

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000000Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
            Assert.AreEqual(0, eventListener.EventCount());
        }

        [Test]
        public async Task Jitter()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).JitterInterval(5_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns, record);

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            Thread.Sleep(5_000);

            query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
        }

        [Test]
        public void ListenWriteSuccessEvent()
        {
            var bucketName = _bucket.Name;

            WriteSuccessEvent success = null;

            _writeApi = Client.GetWriteApi();
            _writeApi.EventHandler += (sender, args) => { success = args as WriteSuccessEvent; };

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
            _writeApi.Flush();

            Thread.Sleep(100);

            Assert.IsNotNull(success);
            Assert.AreEqual(success.Organization, _organization.Id);
            Assert.AreEqual(success.Bucket, bucketName);
            Assert.AreEqual(success.Precision, WritePrecision.Ns);
            Assert.AreEqual(success.LineProtocol, "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
        }

        [Test]
        public void ListenWriteErrorEvent()
        {
            var bucketName = _bucket.Name;

            WriteErrorEvent error = null;

            _writeApi = Client.GetWriteApi();
            _writeApi.EventHandler += (sender, args) => { error = args as WriteErrorEvent; };

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789");
            _writeApi.Flush();

            Thread.Sleep(100);

            Assert.IsNotNull(error);
            Assert.AreEqual(
                "unable to parse points: unable to parse 'h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789': bad timestamp",
                error.Exception.Message);
        }

        [Test]
        public async Task PartialWrite()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi();

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_hill level\\ water_level=2.0 2x";

            _writeApi.WriteRecords(bucketName, _organization.Id, WritePrecision.Ns, record1, record2);
            _writeApi.Flush();
            _writeApi.Dispose();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);
            Assert.AreEqual(0, query.Count);
        }

        [Test]
        public async Task Recovery()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 1x");
            _writeApi.Flush();

            WriteErrorEvent error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(
                "unable to parse points: unable to parse 'h2o_feet,location=coyote_creek level\\ water_level=1.0 1x': bad timestamp",
                error.Exception.Message);

            _writeApi.WriteRecord(bucketName, _organization.Id, WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
            _writeApi.Flush();

            listener.Get<WriteSuccessEvent>();

            var query = await _queryApi.Query(
                "from(bucket:\"" + bucketName + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
            Assert.AreEqual(1, query[0].Records.Count);
            Assert.AreEqual("coyote_creek", query[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("level water_level", query[0].Records[0].GetValueByKey("_field"));
            Assert.AreEqual(1, query[0].Records[0].GetValue());
        }

        [Test]
        public async Task WriteQueryWithDefaultOrgBucket()
        {
            var time = DateTime.UtcNow;

            var point1 = Point
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, WritePrecision.S);

            var point2 = Point
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(point1, point2);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.Query(
                "from(bucket:\"" + _bucket.Name + "\") |> range(start: 1970-01-01T00:00:00.000000001Z)");

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(1, records[1].GetValue());
            Assert.AreEqual("water_level", records[1].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(10)),
                records[0].GetTimeInDateTime());
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), records[1].GetTimeInDateTime());
        }

        [Test]
        public async Task DefaultTagsPoint()
        {
            Client.Dispose();
            
            Environment.SetEnvironmentVariable("point-datacenter", "LA");
            ConfigurationManager.AppSettings["point-sensor.version"] = "1.23a";
            
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                .AuthenticateToken(_token.ToCharArray())
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-var", "${env.point-datacenter}")
                .AddDefaultTag("sensor-version", "${point-sensor.version}")
                .Build();

            Client = InfluxDBClientFactory.Create(options);
            
            var point = Point.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 1);
            
            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoint(_bucket.Name, _organization.Id, point);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.Query(
                "from(bucket:\"" + _bucket.Name + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);
            
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o_feet", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(1, tables[0].Records[0].GetValueByKey("water_level"));
            Assert.AreEqual("west", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("1.23a", tables[0].Records[0].GetValueByKey("sensor-version"));
            Assert.AreEqual("LA", tables[0].Records[0].GetValueByKey("env-var"));
        }

        [Test]
        public async Task DefaultTagsMeasurement()
        {
            Client.Dispose();
            
            Environment.SetEnvironmentVariable("measurement-datacenter", "LA");
            ConfigurationManager.AppSettings["measurement-sensor.version"] = "1.23a";
            
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                .AuthenticateToken(_token.ToCharArray())
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-var", "${env.measurement-datacenter}")
                .AddDefaultTag("sensor-version", "${measurement-sensor.version}")
                .Build();

            Client = InfluxDBClientFactory.Create(options);
            
            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = DateTime.UtcNow
            };
            
            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteMeasurement(_bucket.Name, _organization.Id, WritePrecision.Ms, measurement1);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.Query(
                "from(bucket:\"" + _bucket.Name + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);
            
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(2.927, tables[0].Records[0].GetValueByKey("level"));
            Assert.AreEqual("coyote_creek", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("1.23a", tables[0].Records[0].GetValueByKey("sensor-version"));
            Assert.AreEqual("LA", tables[0].Records[0].GetValueByKey("env-var"));
        }
        
        [Test]
        public async Task DefaultTagsConfiguration()
        {
            Client.Dispose();

            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                .LoadConfig()
                .AuthenticateToken(_token.ToCharArray())
                .Build();
            
            Client = InfluxDBClientFactory.Create(options);
            
            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = DateTime.UtcNow
            };
            
            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteMeasurement(_bucket.Name, _organization.Id, WritePrecision.Ms, measurement1);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.Query(
                "from(bucket:\"" + _bucket.Name + "\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);
            
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(2.927, tables[0].Records[0].GetValueByKey("level"));
            Assert.AreEqual("coyote_creek", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("v1.00", tables[0].Records[0].GetValueByKey("version"));
        }

        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("level")] public double? Level { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}
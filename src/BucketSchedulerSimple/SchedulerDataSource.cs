using System;
using System.Collections.Generic;
using System.Linq;
using TagBites.WinSchedulers;
using TagBites.WinSchedulers.Descriptors;

namespace BucketSchedulerSimple
{
    public class SchedulerDataSource : BucketSchedulerDataSource
    {
        private string[] _rows;
        private ColumnModel[] _columns;
        private readonly IDictionary<(object, object), BucketModel> _buckets = new Dictionary<(object, object), BucketModel>();

        protected override BucketSchedulerBucketDescriptor CreateBucketDescriptor()
        {
            return new BucketSchedulerBucketDescriptor(typeof(BucketModel), nameof(BucketModel.RowResource), nameof(BucketModel.ColumnResource))
            {
                CapacityMember = nameof(BucketModel.Capacity)
            };
        }
        protected override BucketSchedulerTaskDescriptor CreateTaskDescriptor()
        {
            return new BucketSchedulerTaskDescriptor(typeof(TaskModel), nameof(TaskModel.Bucket))
            {
                ConsumptionMember = nameof(TaskModel.Consumption)
            };
        }

        public override IList<object> LoadRows() => (_rows ?? (_rows = GenerateRows())).Cast<object>().ToList();
        public override IList<object> LoadColumns() => (_columns ?? (_columns = GenerateColumns())).Cast<object>().ToList();
        public override void LoadContent(BucketSchedulerDataSourceView view)
        {
            var rows = view.Rows;
            var columns = view.Columns;

            foreach (var row in rows)
                foreach (var column in columns)
                {
                    if (!_buckets.ContainsKey((row, column)))
                        _buckets.Add((row, column), GenerateBucket(row, column));

                    var bucket = _buckets[(row, column)];
                    view.AddBucket(bucket);
                    foreach (var task in bucket.Tasks)
                        view.AddTask(task);
                }
        }

        #region Data generation

        private readonly Random _random = new Random();

        private string[] GenerateRows()
        {
            return new[]
            {
                "[A] Cutting Station",
                "[B] Preparation Station",
                "[C] Bonding Station",
                "[D] Testing Station",
                "[E] Painting/Lacquering Station",
                "[F] Decorating Station",
                "[G] Controlling Station",
                "[H] Packing Station"
            };
        }
        private ColumnModel[] GenerateColumns()
        {
            var date = DateTime.Now.Date;
            return Enumerable.Range(0, 365).Select(x => new ColumnModel { Date = date + TimeSpan.FromDays(x) }).ToArray();
        }
        private BucketModel GenerateBucket(object row, object column)
        {
            var bucket = new BucketModel
            {
                RowResource = row,
                ColumnResource = column,
                Capacity = _random.NextDouble() * 100
            };

            var count = _random.Next(0, 10);
            for (var i = 0; i < count; i++)
            {
                bucket.Tasks.Add(new TaskModel
                {
                    ID = i,
                    Bucket = bucket,
                    Consumption = _random.NextDouble() * 10,
                });
            }

            return bucket;
        }

        #endregion

        #region Classes

        private class ColumnModel
        {
            public DateTime Date { get; set; }

            public override string ToString() => Date.ToString("yyyy-MM-dd");
        }
        private class BucketModel
        {
            public object RowResource { get; set; }
            public object ColumnResource { get; set; }
            public List<TaskModel> Tasks { get; } = new List<TaskModel>();
            public double Capacity { get; set; }

            public override string ToString()
            {
                return Tasks.Count > 1 ?  $"{Tasks.Count} tasks" : "";
            }
        }
        private class TaskModel
        {
            public int ID { get; set; }
            public BucketModel Bucket { get; set; }
            public double Consumption { get; set; }

            public override string ToString()
            {
                return $"Quantity: {Consumption:#,0.00} units";
            }
        }

        #endregion
    }
}

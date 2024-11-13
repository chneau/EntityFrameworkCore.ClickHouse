using EntityFrameworkCore.ClickHouse.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace EntityFrameworkCore.ClickHouse.FunctionalTests;

public class BuiltInDataTypesClickHouseTest : BuiltInDataTypesTestBase<BuiltInDataTypesClickHouseTest.BuiltInDataTypesClickHouseFixture>
{
    public BuiltInDataTypesClickHouseTest(BuiltInDataTypesClickHouseFixture fixture) : base(fixture)
    {
    }

    private void QueryBuiltInDataTypesTest<TEntity>(EntityEntry<TEntity> source)
        where TEntity : BuiltInDataTypesBase
    {
        using var context = CreateContext();
        var set = context.Set<TEntity>();
        var entity = set.Where(e => e.Id == 11).ToList().Single();
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        var param1 = (short)-1234;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<short>(e, nameof(BuiltInDataTypes.TestInt16)) == param1).ToList().Single());

        var param2 = -123456789;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<int>(e, nameof(BuiltInDataTypes.TestInt32)) == param2).ToList().Single());

        var param3 = -1234567890123456789L;
        if (Fixture.IntegerPrecision == 64)
        {
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<long>(e, nameof(BuiltInDataTypes.TestInt64)) == param3).ToList().Single());
        }

        double? param4 = -1.23456789;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, set.Where(
                    e => e.Id == 11
                         && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4).ToList().Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            double? param4l = -1.234567891;
            double? param4h = -1.234567889;
            Assert.Same(
                entity, set.Where(
                        e => e.Id == 11
                             && (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4
                                 || (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) > param4l
                                     && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) < param4h)))
                    .ToList().Single());
        }

        var param5 = -1234567890.01M;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<decimal>(e, nameof(BuiltInDataTypes.TestDecimal)) == param5).ToList()
                .Single());

        var param6 = Fixture.DefaultDateTime;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<DateTime>(e, nameof(BuiltInDataTypes.TestDateTime)) == param6).ToList()
                .Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestDateTimeOffset)) != null)
        {
            var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<DateTimeOffset>(e, nameof(BuiltInDataTypes.TestDateTimeOffset)) == param7)
                    .ToList().Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestTimeSpan)) != null)
        {
            var param8 = new TimeSpan(0, 10, 9, 8, 7);
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<TimeSpan>(e, nameof(BuiltInDataTypes.TestTimeSpan)) == param8).ToList()
                    .Single());
        }

        var param9 = -1.234F;
        if (Fixture.StrictEquality)
        {
            Assert.Same(
                entity, set.Where(
                    e => e.Id == 11
                         && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param9).ToList().Single());
        }
        else if (Fixture.SupportsDecimalComparisons)
        {
            var param9l = -1.2341F;
            var param9h = -1.2339F;
            Assert.Same(
                entity, set.Where(
                    e => e.Id == 11
                         && (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param9
                             || (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) > param9l
                                 && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) < param9h))).ToList().Single());
        }

        var param10 = true;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<bool>(e, nameof(BuiltInDataTypes.TestBoolean)) == param10).ToList().Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestByte)) != null)
        {
            var param11 = (byte)255;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<byte>(e, nameof(BuiltInDataTypes.TestByte)) == param11).ToList().Single());
        }

        var param12 = Enum64.SomeValue;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param12).ToList().Single());

        var param13 = Enum32.SomeValue;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param13).ToList().Single());

        var param14 = Enum16.SomeValue;
        Assert.Same(
            entity,
            set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param14).ToList().Single());

        if (entityType.FindProperty(nameof(BuiltInDataTypes.Enum8)) != null)
        {
            var param15 = Enum8.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param15).ToList().Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt16)) != null)
        {
            var param16 = (ushort)1234;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<ushort>(e, nameof(BuiltInDataTypes.TestUnsignedInt16)) == param16).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt32)) != null)
        {
            var param17 = 1234565789U;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<uint>(e, nameof(BuiltInDataTypes.TestUnsignedInt32)) == param17).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt64)) != null)
        {
            var param18 = 1234567890123456789UL;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<ulong>(e, nameof(BuiltInDataTypes.TestUnsignedInt64)) == param18).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestCharacter)) != null)
        {
            var param19 = 'a';
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<char>(e, nameof(BuiltInDataTypes.TestCharacter)) == param19).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.TestSignedByte)) != null)
        {
            var param20 = (sbyte)-128;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<sbyte>(e, nameof(BuiltInDataTypes.TestSignedByte)) == param20).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU64)) != null)
        {
            var param21 = EnumU64.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<EnumU64>(e, nameof(BuiltInDataTypes.EnumU64)) == param21).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU32)) != null)
        {
            var param22 = EnumU32.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<EnumU32>(e, nameof(BuiltInDataTypes.EnumU32)) == param22).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU16)) != null)
        {
            var param23 = EnumU16.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<EnumU16>(e, nameof(BuiltInDataTypes.EnumU16)) == param23).ToList()
                    .Single());
        }

        if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumS8)) != null)
        {
            var param24 = EnumS8.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<EnumS8>(e, nameof(BuiltInDataTypes.EnumS8)) == param24).ToList().Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum64))?.GetProviderClrType()) == typeof(long))
        {
            var param25 = 1;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == (Enum64)param25).ToList()
                    .Single());
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && (int)EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param25).ToList()
                    .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum32))?.GetProviderClrType()) == typeof(int))
        {
            var param26 = 1;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == (Enum32)param26).ToList()
                    .Single());
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && (int)EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param26).ToList()
                    .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum16))?.GetProviderClrType()) == typeof(short))
        {
            var param27 = 1;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == (Enum16)param27).ToList()
                    .Single());
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && (int)EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param27).ToList()
                    .Single());
        }

        if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum8))?.GetProviderClrType()) == typeof(byte))
        {
            var param28 = 1;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == (Enum8)param28).ToList()
                    .Single());
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && (int)EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param28).ToList()
                    .Single());
        }

        AssertProperties(source, context, entity);
    }

    private void AssertProperties<TEntity>(EntityEntry<TEntity> source, DbContext context, TEntity entity) where TEntity : class
    {
        foreach (var propertyEntry in context.Entry(entity).Properties)
        {
            if (propertyEntry.Metadata.ValueGenerated != ValueGenerated.Never)
            {
                continue;
            }

            if (propertyEntry.CurrentValue is double)
            {
                Assert.Equal(
                    (double)source.Property(propertyEntry.Metadata.Name).CurrentValue,
                    (double)propertyEntry.CurrentValue,
                    Fixture.DoublePrecision);
            }
            else if (propertyEntry.CurrentValue is decimal)
            {
                Assert.Equal(
                    (decimal)source.Property(propertyEntry.Metadata.Name).CurrentValue,
                    (decimal)propertyEntry.CurrentValue,
                    Fixture.DecimalPrecision);
            }
            else if (propertyEntry.CurrentValue is Array && source.Property(propertyEntry.Metadata.Name).CurrentValue is null)
            {
                Assert.Empty((IEnumerable)propertyEntry.CurrentValue);
            }
            else
            {
                Assert.Equal(
                    source.Property(propertyEntry.Metadata.Name).CurrentValue,
                    propertyEntry.CurrentValue);
            }
        }
    }

    private static Type UnwrapNullableType(Type type)
        => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;

    public class BuiltInDataTypesClickHouseFixture : BuiltInDataTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ClickHouseTestStoreFactory.Instance;

        public override bool StrictEquality => false;

        public override bool SupportsAnsi => true;

        public override bool SupportsUnicodeToAnsiConversion => false;

        public override bool SupportsLargeStringComparisons => false;

        public override bool SupportsBinaryKeys => false;

        public override bool SupportsDecimalComparisons => true;

        public override DateTime DefaultDateTime => DateTime.UnixEpoch;

        public override bool PreservesDateTimeKind => true;

        public int DecimalPrecision { get; set; } = 1;

        public int DoublePrecision { get; set; } = 8;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<MaxLengthDataTypes>().Property(e => e.ByteArray5).IsRequired();
            modelBuilder.Entity<MaxLengthDataTypes>().Property(e => e.ByteArray9000).IsRequired();
            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.TestDecimal).HasPrecision(18, 4);
            modelBuilder.Entity<BuiltInNullableDataTypes>().Property(e => e.TestNullableDecimal).HasPrecision(18, 4);
            modelBuilder.Entity<NonNullableBackedDataTypes>().Property(e => e.Decimal).HasPrecision(18, 4);
            modelBuilder.Entity<NullableBackedDataTypes>().Property(e => e.Decimal).HasPrecision(18, 4);
            modelBuilder.Entity<BuiltInNullableDataTypes>().Ignore(e => e.TestByteArray);
            modelBuilder.Entity<ObjectBackedDataTypes>().Property(e => e.Bytes).IsRequired();
            modelBuilder.Entity<ObjectBackedDataTypes>().Property(e => e.Decimal).HasPrecision(18, 4);
            modelBuilder.Entity<BuiltInDataTypesShadow>().Property("TestDecimal").HasPrecision(18, 4);
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            return base.AddOptions(builder).LogTo(s => Trace.WriteLine(s));
        }
    }
}

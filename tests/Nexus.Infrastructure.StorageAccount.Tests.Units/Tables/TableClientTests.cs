using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Core.Tests;
using Nexus.Infrastructure.StorageAccount.Tables.Client;

namespace Nexus.Infrastructure.StorageAccount.Tests.Units.Tables;

public class TableClientTests : ClientTestsBase, IClassFixture<TableClientFixture>
{
    private readonly ITableClient<TestTableStorageConfiguration, TestTableEntity> _tableClient;

    public TableClientTests(TableClientFixture fixture)
    {
        var sp = fixture.ServiceProvider;
        _tableClient = sp.GetService<ITableClient<TestTableStorageConfiguration, TestTableEntity>>();
    }

    [Fact()]
    public async Task Exists_WhenRowDoesNotExist_ThenNullIsReturned()
    {
        // Act
        var result = await _tableClient.ExistsAsync(RandomValue.String, RandomValue.String);

        // Assert
        result.Should().BeFalse();
    }

    [Fact()]
    public async Task Exists_WhenRowExists_ThenEntityIsReturned()
    {
        var entity = GetEntity();

        await _tableClient.AddAsync(entity);

        // Clean up
        AddCleanupAction(() => _tableClient.DeleteAsync(entity));

        // Act
        var result = await _tableClient.ExistsAsync(entity.PartitionKey, entity.RowKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact()]
    public async Task Upsert_WhenAnEntityIsUpserted_AndThereIsNoExistingRow_ThenEntityIsAdded()
    {
        var entity = GetEntity();

        // Clean up
        AddCleanupAction(() => _tableClient.DeleteAsync(entity));

        // Act
        await _tableClient.UpsertAsync(entity);

        var maybe = await _tableClient.GetByIdAsync(entity.PartitionKey, entity.RowKey);

        // Assert
        maybe.Value.Should().BeEquivalentTo(entity, EntityEquivalencyWhitelist);
    }

    [Fact()]
    public async Task Upsert_WhenAnEntityIsUpserted_AndTheEntityAlreadyExists_ThenTheExistingRecordIsOverriden()
    {
        var existing = GetEntity();

        await _tableClient.AddAsync(existing);

        // Clean up
        AddCleanupAction(() => _tableClient.DeleteAsync(existing));

        // Act
        existing.TestData = RandomValue.String;

        await _tableClient.UpsertAsync(existing);
        var maybe = await _tableClient.GetByIdAsync(existing.PartitionKey, existing.RowKey);

        // Assert
        maybe.Value.Should().BeEquivalentTo(existing, EntityEquivalencyWhitelist);
    }

    private EquivalencyAssertionOptions<TestTableEntity> EntityEquivalencyWhitelist(EquivalencyAssertionOptions<TestTableEntity> _)
    {
        return _.Including(x => x.PartitionKey)
            .Including(x => x.RowKey)
            .Including(x => x.TestData);
    }

    private TestTableEntity GetEntity(string partitionKey = null, string rowKey = null, string testData = null)
    {
        return new TestTableEntity
        {
            PartitionKey = partitionKey ?? RandomValue.String,
            RowKey = rowKey ?? RandomValue.String,
            TestData = testData ?? RandomValue.String
        };
    }
}
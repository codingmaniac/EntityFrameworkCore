// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.ChangeTracking.Internal
{
    public class InternalEntryEntrySubscriberTest
    {
        [Fact]
        public void Snapshots_are_created_for_entities_without_changing_notifications()
        {
            var entity = new ChangedOnlyNotificationEntity { Name = "Palmer", Id = 1 };
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                entity);

            Assert.True(entry.HasRelationshipSnapshot);

            Assert.Equal("Palmer", entry.GetOriginalValue(entry.EntityType.FindProperty("Name")));

            entity.Name = "Luckey";

            Assert.Equal("Palmer", entry.GetOriginalValue(entry.EntityType.FindProperty("Name")));
        }

        [Fact]
        public void Snapshots_are_not_created_for_full_notification_entities()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry<FullNotificationEntity>(BuildModel());
            entry.SetEntityState(EntityState.Unchanged);

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.HasRelationshipSnapshot);
        }

        [Fact]
        public void Relationship_snapshot_is_created_when_entity_has_non_notifying_collection_instance()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullNotificationEntity { Name = "Palmer", Id = 1, RelatedCollection = new List<ChangedOnlyNotificationEntity>() });

            Assert.True(entry.HasRelationshipSnapshot);
        }

        [Fact]
        public void Relationship_snapshot_is_not_created_when_entity_has_notifying_collection()
        {
            var entry = TestHelpers.Instance.CreateInternalEntry(
                BuildModel(),
                EntityState.Unchanged,
                new FullNotificationEntity { Id = -1, Name = "Palmer", RelatedCollection = new ObservableCollection<ChangedOnlyNotificationEntity>() });

            // TODO: The following assert should be changed to False once INotifyCollectionChanged is supported (Issue #445)
            Assert.True(entry.HasRelationshipSnapshot);
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.Name = "Palmer";

            var property = entry.EntityType.FindProperty("Name");
            Assert.Same(property, testListener.Changing.Single());
            Assert.Same(property, testListener.Changed.Single());
        }

        [Fact]
        public void Entry_handles_null_or_empty_string_in_INotifyPropertyChanging_and_INotifyPropertyChanged()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotifyChanging(null);

            Assert.Equal(
                new[] { "Name", "RelatedCollection" }, 
                testListener.Changing.Select(e => e.Name).OrderBy(e => e).ToArray());

            Assert.Empty(testListener.Changed);

            entity.NotifyChanged("");

            Assert.Equal(
                new[] { "Name", "RelatedCollection" },
                testListener.Changed.Select(e => e.Name).OrderBy(e => e).ToArray());
        }

        [Fact]
        public void Entry_subscribes_to_INotifyPropertyChanging_and_INotifyPropertyChanged_for_navigations()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);
            entry.SetEntityState(EntityState.Unchanged);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.RelatedCollection = new List<ChangedOnlyNotificationEntity>();

            var property = entry.EntityType.FindNavigation("RelatedCollection");
            Assert.Same(property, testListener.Changing.Single());
            Assert.Same(property, testListener.Changed.Single());
        }

        [Fact]
        public void Subscriptions_to_INotifyPropertyChanging_and_INotifyPropertyChanged_ignore_unmapped_properties()
        {
            var contextServices = TestHelpers.Instance.CreateContextServices(
                new ServiceCollection().AddScoped<IPropertyListener, TestPropertyListener>(),
                BuildModel());

            var testListener = contextServices.GetRequiredService<IEnumerable<IPropertyListener>>().OfType<TestPropertyListener>().Single();

            var entity = new FullNotificationEntity();
            contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(entity);

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);

            entity.NotMapped = "Luckey";

            Assert.Empty(testListener.Changing);
            Assert.Empty(testListener.Changed);
        }

        private class TestPropertyListener : IPropertyListener
        {
            public List<IPropertyBase> Changing { get; } = new List<IPropertyBase>();
            public List<IPropertyBase> Changed { get; } = new List<IPropertyBase>();

            public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified) 
                => Changed.Add(property);

            public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property) 
                => Changing.Add(property);
        }

        private static IModel BuildModel()
        {
            var builder = TestHelpers.Instance.CreateConventionBuilder();

            builder.Entity<FullNotificationEntity>(b =>
                {
                    b.Ignore(e => e.NotMapped);
                    b.HasMany(e => e.RelatedCollection).WithOne(e => e.Related).HasForeignKey(e => e.Fk);
                });

            return builder.Model;
        }

        private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private string _notMapped;
            private ICollection<ChangedOnlyNotificationEntity> _relatedCollection;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public string NotMapped
            {
                get { return _notMapped; }
                set { SetWithNotify(value, ref _notMapped); }
            }

            public ICollection<ChangedOnlyNotificationEntity> RelatedCollection
            {
                get { return _relatedCollection; }
                set { SetWithNotify(value, ref _relatedCollection); }
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    NotifyChanging(propertyName);
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public void NotifyChanging(string propertyName)
                => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        private class ChangedOnlyNotificationEntity : INotifyPropertyChanged
        {
            private int _id;
            private string _name;
            private int _fk;
            private FullNotificationEntity _related;

            public int Id
            {
                get { return _id; }
                set { SetWithNotify(value, ref _id); }
            }

            public string Name
            {
                get { return _name; }
                set { SetWithNotify(value, ref _name); }
            }

            public int Fk
            {
                get { return _fk; }
                set { SetWithNotify(value, ref _fk); }
            }

            public FullNotificationEntity Related
            {
                get { return _related; }
                set { SetWithNotify(value, ref _related); }
            }

            private void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
            {
                if (!StructuralComparisons.StructuralEqualityComparer.Equals(field, value))
                {
                    field = value;
                    NotifyChanged(propertyName);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

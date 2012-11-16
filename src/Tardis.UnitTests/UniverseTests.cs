// The MIT License
// 
// Copyright (c) 2012 Jordan E. Terrell
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using FluentAssertions;
using NUnit.Framework;

namespace Tardis
{
    [TestFixture]
    public class UniverseTests
    {
        [Test]
        public void Create_ReturnsNewUniverse()
        {
            var universe = Universe.Create();
            universe.Should().NotBeNull();
        }

        [Test]
        public void Create_ReturnsUniverseWithOneChangeSet()
        {
            var universe = Universe.Create();
            universe.LastChangeSet.Should().NotBeNull();
            universe.LastChangeSet.Previous.HasValue.Should().BeFalse();
        }

        [Test]
        public void ObserveCommittedChangeSets_WithNewUniverse_AndObservingHistorical_YieldsOneChangeSetOnSubscribe()
        {
            var list = new List<ChangeSet>();
            var universe = Universe.Create();

            using (universe.ObserveCommittedChangeSets(true)
                .Subscribe(Observer.Create<ChangeSet>(list.Add)))
            {
                list.Should().HaveCount(1);
                list[0].Changes.First()
                    .Should().BeAssignableTo<Universe.Created>();
            }
        }

        [Test]
        public void ObserveCommittedChangeSets_WithNewUniverse_AndNotObservingHistorical_YieldsNoChangeSetsOnSubscribe()
        {
            var list = new List<ChangeSet>();
            var universe = Universe.Create();

            using (universe.ObserveCommittedChangeSets(false)
                .Subscribe(Observer.Create<ChangeSet>(list.Add)))
            {
                list.Should().HaveCount(0);
            }
        }

        [Test]
        public void Annotate_OutsideOfChange_ThrowsException()
        {
            var universe = Universe.Create();

            universe.Invoking(u => u.Annotate(new Comment("Hello, World!")))
                    .ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Annotate_WithinChange_CommitsNewChangeSet()
        {
            var universe = Universe.Create();

            universe.Change(1, u => u.Annotate(new Comment("Hello, World!")));
            universe.Version.Should().Be(2);

            var annotatedChange = (Node.Annotated)universe.LastChangeSet.Single();
            var annotation = (Comment) annotatedChange.Annotation;

            annotation.Message.Should().Be("Hello, World!");
        }
    }
}

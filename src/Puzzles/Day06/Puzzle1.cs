using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day06
{
    public class Puzzle1 : IPuzzle
    {
        public int Solve()
        {
            var orbits = this.GetOrbits().ToList();

            var spaceObjects = new Dictionary<string, SpaceObject>();

            foreach (var orbit in orbits)
            {
                if (!spaceObjects.TryGetValue(orbit.BodyId, out var body))
                {
                    body = new SpaceObject(orbit.BodyId);
                    spaceObjects.Add(orbit.BodyId, body);
                }

                if (!spaceObjects.TryGetValue(orbit.SatelliteId, out var satellite))
                {
                    satellite = new SpaceObject(orbit.SatelliteId);
                    spaceObjects.Add(orbit.SatelliteId, satellite);
                }

                body.AddSatellite(satellite);
            }

            var checksum = spaceObjects.Values.Sum(x => x.Level);

            return checksum;
        }

        private IEnumerable<OrbitData> GetOrbits()
        {
            var lines = File.ReadAllLines("Puzzles\\Day06\\input.txt");

            foreach (var line in lines)
            {
                var objectIds = line.Split(')');
                var bodyId = objectIds[0];
                var satelliteId = objectIds[1];

                yield return new OrbitData(bodyId, satelliteId);
            }
        }

        private class OrbitData
        {
            public OrbitData(string bodyId, string satelliteId)
            {
                BodyId = bodyId;
                SatelliteId = satelliteId;
            }

            public string BodyId { get; }

            public string SatelliteId { get; }
        }

        public class SpaceObject
        {
            private readonly IList<SpaceObject> _satellites = new List<SpaceObject>();

            public SpaceObject(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public SpaceObject Parent { get; private set; }

            public IEnumerable<SpaceObject> Satellites => _satellites;

            public int Level => Parent?.Level + 1 ?? 0;

            public void AddSatellite(SpaceObject satellite)
            {
                _satellites.Add(satellite);

                satellite.Parent = this;
            }
        }
    }
}
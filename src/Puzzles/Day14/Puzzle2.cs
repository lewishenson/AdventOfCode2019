using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2019.Puzzles.Day14
{
    public class Puzzle2 : IPuzzle
    {
        public object Solve()
        {
            var reactions = GetReactions().ToList();

            var nanoFactory = new NanoFactory(reactions);
            
            const long availableOre = 1000000000000;
            
            long fuelCount = 0;
            var fuelCreationStep = 1000000;
            
            while (fuelCreationStep >= 1)
            {
                while (nanoFactory.OreUsed < availableOre)
                {
                    nanoFactory.Create("FUEL", fuelCreationStep);
                    fuelCount += fuelCreationStep;
                }

                if (fuelCreationStep >= 1)
                {
                    nanoFactory.Rollback();
                    fuelCount -= fuelCreationStep;
                    fuelCreationStep /= 10;
                }
            }

            return fuelCount;
        }

        private IEnumerable<Reaction> GetReactions()
        {
            var lines = File.ReadAllLines("Puzzles/Day14/input.txt");

            return lines.Select(ParseLine);
        }

        private Reaction ParseLine(string line)
        {
            var reactionSides = line.Split('>');

            var reactants = new List<Molecule>();

            Molecule ToMolecule(string input)
            {
                var rawProductParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var name = rawProductParts[1].Trim();
                var quantity = int.Parse(rawProductParts[0]);
                return new Molecule(name, quantity);
            }

            var rawReactants = reactionSides[0].Split(new[] { ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawReactant in rawReactants)
            {
                var reactant = ToMolecule(rawReactant);
                reactants.Add(reactant);
            }

            var product = ToMolecule(reactionSides[1]);

            return new Reaction(reactants, product);
        }

        private class NanoFactory
        {
            private readonly IReadOnlyDictionary<string, Reaction> _reactions;
            private IDictionary<string, long> _stock;
            private long _snapshotOreUsed;
            private IDictionary<string, long> _snapshotStock;

            public NanoFactory(IReadOnlyCollection<Reaction> reactions)
            {
                _reactions = reactions.ToDictionary(r => r.Product.Name, r => r);
                _stock = reactions.Select(r => r.Product.Name).ToDictionary(r => r, _ => (long)0);
            }

            public long OreUsed { get; private set; }

            public void Create(string name, long quantity)
            {
                if (name == "FUEL")
                {
                    TakeSnapshot();
                }
                else if (name == "ORE")
                {
                    OreUsed += quantity;
                    return;
                }

                // If already made, no work to do.
                if (_stock[name] >= quantity)
                {
                    _stock[name] -= quantity;
                    return;
                }

                // Can use all existing stock even if it doesn't completely fulfill the request.
                var stockAdjustedQuantity = quantity - _stock[name];
                _stock[name] = 0;

                var reaction = _reactions[name];

                var batchSize = (int)Math.Ceiling((double)stockAdjustedQuantity / reaction.Product.Quantity);

                foreach (var reactant in reaction.Reactants)
                {
                    Create(reactant.Name, reactant.Quantity * batchSize);
                }

                // Save any used stock for next time.
                var actualQuantityMade = batchSize * reaction.Product.Quantity;
                var unusedQuantity = actualQuantityMade - stockAdjustedQuantity;
                _stock[name] += unusedQuantity;
            }

            private void TakeSnapshot()
            {
                _snapshotOreUsed = OreUsed;
                _snapshotStock = new Dictionary<string, long>(_stock);
            }

            public void Rollback()
            {
                OreUsed = _snapshotOreUsed;
                _stock = new Dictionary<string, long>(_snapshotStock);

                _snapshotOreUsed = 0;
                _snapshotStock = null;
            }
        }

        private class Molecule
        {
            public Molecule(string name, long quantity)
            {
                Name = name;
                Quantity = quantity;
            }

            public string Name { get; }

            public long Quantity { get; }

            public override string ToString() => $"{Quantity} {Name}";
        }

        private class Reaction
        {
            private readonly IList<Molecule> _reactants;

            public Reaction(IEnumerable<Molecule> reactants, Molecule product)
            {
                _reactants = reactants.ToList();
                Product = product;
            }

            public IEnumerable<Molecule> Reactants => _reactants;

            public Molecule Product { get; }

            public override string ToString()
            {
                var joinedReactants = string.Join(", ", _reactants);

                return joinedReactants.Trim() + " => " + Product;
            }
        }
    }
}
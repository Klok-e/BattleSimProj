using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Battle_sim_imp
{
    public class Sim
    {
        public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
        {
            blood = 1000,
            str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
        };

        System.Random random;

        public const int blocksAiSeesInEachDir = 3;

        Block[,] map;
        List<Body> bodies;
        List<Warrior> warriors;

        public Sim(int height, int width, int warrOnEachSide)
        {
            random = new System.Random();
            //generate map
            map = new Block[height, width];
            bodies = new List<Body>();
            warriors = new List<Warrior>();
            
            for (int i = 0; i < warrOnEachSide; i++)
            {
                var w = new Warrior(defaultWarStrct, map, new Vector2(1, 1), 0, new RandomAi(9001, 5, random));
                warriors.Add(w);
            }
            for (int i = 0; i < warrOnEachSide; i++)
            {
                var w = new Warrior(defaultWarStrct, map, new Vector2(width - 2, height - 2), 1, new RandomAi(9001, 5, random));
                warriors.Add(w);
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = new Block(map, new Vector2(i, j)) { empty = (i == 0 || i == height - 1 || j == 0 || j == width - 1) ? false : true };
                }
            }
        }

        public void Tick()
        {
            foreach (var body in bodies)
            {
                body.Tick();
            }

            foreach (var warrior in warriors)
            {
                if (!warrior.currentAction.isFinished)
                    warrior.Tick();
                else
                    warrior.Tick(ChooseActionForWarrior(warrior));
            }
        }

        Actions.Action ChooseActionForWarrior(Warrior warrior)
        {
            var inputsForAi = new List<double>();

            var blocksSees = new double[7, 7];

            var otherWarriorsSeesTeams = new double[7, 7];
            var otherWarriorsSeesActions = new double[7, 7];
            var otherWarriorsSeesTicksToFinishAction = new double[7, 7];

            for (int i = (int)warrior.BlockThisAt.pos.x - blocksAiSeesInEachDir; i <= (int)warrior.BlockThisAt.pos.x + blocksAiSeesInEachDir; i++)
            {
                for (int j = (int)warrior.BlockThisAt.pos.y - blocksAiSeesInEachDir; j <= (int)warrior.BlockThisAt.pos.y + blocksAiSeesInEachDir; j++)
                {
                    blocksSees[i, j] = map[i, j].empty ? 0 : 1;
                    foreach (var warr in warriors)
                    {
                        if (warr.BlockThisAt.pos == map[i, j].pos)
                        {
                            otherWarriorsSeesActions[i, j] = warr.GetActionNum();
                            otherWarriorsSeesTicksToFinishAction[i, j] = warr.currentAction.ticksToFinish;
                            otherWarriorsSeesTeams[i, j] = warr.team;
                        }
                    }
                }
            }

            #region Flatten
            foreach (var item in blocksSees)
            {
                inputsForAi.Add(item);
            }
            foreach (var item in otherWarriorsSeesTeams)
            {
                inputsForAi.Add(item);
            }
            foreach (var item in otherWarriorsSeesActions)
            {
                inputsForAi.Add(item);
            }
            foreach (var item in otherWarriorsSeesTicksToFinishAction)
            {
                inputsForAi.Add(item);
            }
            #endregion

            var prediction = warrior.ai.Predict(inputsForAi.ToArray());
            //first 5 correspond to which action to choose; 6 - which direction to go in radians

            int mxInd = 0;
            double mx = prediction[mxInd];
            for (int i = 0; i < prediction.Length - 1; i++)
            {
                if (prediction[i] > mx)
                {
                    mx = prediction[i];
                    mxInd = i;
                }
            }

            float angle = (float)prediction[5];
            var vectorxy = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var blockLooksAt = map[(int)Mathf.Round(warrior.BlockThisAt.pos.x + vectorxy.x), (int)Mathf.Round(warrior.BlockThisAt.pos.y + vectorxy.y)];

            switch (mxInd)
            {
                case 0:
                    if (blockLooksAt.warriorsAtThis.Count > 0)
                    {
                        var lmb = blockLooksAt.warriorsAtThis[0].limbs[random.Next(0, blockLooksAt.warriorsAtThis[0].limbs.Count)];
                        return new Actions.AttackAction(lmb, 10, 50);
                    }
                    break;
                case 1:
                    return new Actions.DodgeAction();
                case 2:
                    return new Actions.BlockAction();
                case 3:
                    return new Actions.MoveAction(warrior.speed, blockLooksAt, warrior);
                case 4:
                    return new Actions.IdleAction();
            }
            throw new Exception("shi");
        }

        public void ExportData(List<BlockData> blocks, List<WarrData> warrs, List<BodyData> bods)
        {
            blocks.Clear();
            warrs.Clear();
            bods.Clear();
            foreach (var item in map)
            {
                blocks.Add(new BlockData(item));
            }
            foreach (var item in warriors)
            {
                warrs.Add(new WarrData(item));
            }
            foreach (var item in bodies)
            {
                bods.Add(new BodyData(item));
            }
        }
    }

    public class Block
    {
        Block[,] mapThisAt;
        List<Body> bodiesAtThis;
        public List<Warrior> warriorsAtThis;

        public Vector2 pos { get; }

        public bool empty = true;

        public Block(Block[,] mp, Vector2 ps)
        {
            pos = ps;
            mapThisAt = mp;
        }
    }

    public class Body
    {
        Block[,] mapThisAt;
        Vector2 velocity;
        public Vector2 pos;
        int ticksToFall;

        public Body(Vector2 pos, Vector2 velocity, int ticksLeftToFall, Block[,] map)
        {
            this.pos = pos;
            this.velocity = velocity;
            ticksToFall = ticksLeftToFall;

            mapThisAt = map;
        }

        public void Tick()
        {
            pos += velocity;
            velocity.Scale(new Vector2(0.9f, 0.9f));
        }
    }

    public static class AddFuncs
    {
        public static TSource MaxBy<TSource, TProperty>(this IEnumerable<TSource> source, Func<TSource, TProperty> selector)
        {
            // check args        

            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException();

                var max = iterator.Current;
                var maxValue = selector(max);
                var comparer = Comparer<TProperty>.Default;

                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var currentValue = selector(current);

                    if (comparer.Compare(currentValue, maxValue) > 0)
                    {
                        max = current;
                        maxValue = currentValue;
                    }
                }

                return max;
            }
        }
    }
}

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
        public const int blocksAiSeesInEachDir = 3;

        Block[,] map;
        List<Body> bodies;
        List<Warrior> warriors;

        public Sim(int height, int width)
        {
            //generate map
            map = new Block[height, width];

            for (int i = 0; i < map.GetLength(1); i++)
            {
                for (int j = 0; j < map.GetLength(2); j++)
                {
                    map[i, j] = new Block(map, new Vector2(i, j));
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
                        var lmb = blockLooksAt.warriorsAtThis[0].limbs[UnityEngine.Random.Range(0, blockLooksAt.warriorsAtThis[0].limbs.Count)];
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
        Vector2 pos;
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

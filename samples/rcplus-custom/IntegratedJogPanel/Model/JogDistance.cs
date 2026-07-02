// -----------------------------------------------------------------------
// <copyright file="JogDistance.cs" company="Seiko Epson Corporation">
// Copyright (C) Seiko Epson Corporation 2026, All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using static Epson.RoboticsShared.ExtensionsAPI.IRCXRobotManagerAPI;
using V2 = Epson.RoboticsShared.ExtensionsAPI.V2;

namespace IntegratedJogPanel.Model
{
    /// <summary>
    /// Jog disatance.
    /// </summary>
    public class JogDistance
    {
        public RCXJogMode JogMode { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Labels
        /// </summary>
        public string[] Labels { get; }
        
        /// <summary>
        /// Units
        /// </summary>
        public string[] Units { get; }

        /// <summary>
        /// Long jog distance
        /// </summary>
        public double[] DistanceL { get; } = new double[6];

        /// <summary>
        /// Medium jog distance
        /// </summary>
        public double[] DistanceM { get; } = new double[6];

        /// <summary>
        /// Short jog distance
        /// </summary>
        public double[] DistanceS { get; } = new double[6];

        /// <summary>
        /// Selected jog distance type
        /// </summary>
        public RCXJogDistance SelectedType { get; set; } = RCXJogDistance.Continuous;

        private V2.IRCXRobotManagerAPI _robotManagerAPI_V2;

        /// <summary>
        /// Selected jog distance type words
        /// </summary>
        public List<(RCXJogDistance type, string word)> SelectedTypeWords { get; } = new List<(RCXJogDistance, string)>()
        {
            (RCXJogDistance.Continuous, "Cont"),
            (RCXJogDistance.Long, "Long"),
            (RCXJogDistance.Middle, "Mid"),
            (RCXJogDistance.Short, "Short"),
        };

        /// <summary>
        /// Constructor of JogDistance.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="labels">Labels words separated by commma</param>
        /// <param name="units">Units words separated by commma</param>
        public JogDistance(RCXJogMode jogMode, string name, string labels, string units)
        {
            _robotManagerAPI_V2 = Main.GetAPI<V2.IRCXRobotManagerAPI>();

            JogMode = jogMode;
            Name = name;
            Labels = labels.Split(",");
            Units = units.Split(",");
        }

        /// <summary>
        /// Select jog type.
        /// </summary>
        /// <param name="word">Jog type</param>
        public void SelectType(string word)
        {
            SelectedType = SelectedTypeWords.FirstOrDefault(i => i.word == word).type;
        }

        /// <summary>
        /// Load jog distance.
        /// </summary>
        /// <param name="type">Distance type</param>
        /// <returns></returns>
        public async Task Load(RCXJogDistance type)
        {
            double[]? target = type switch
            {
                RCXJogDistance.Long => DistanceL,
                RCXJogDistance.Middle => DistanceM,
                RCXJogDistance.Short => DistanceS,
                _ => null,
            };

            if (target == null)
            {
                return;
            }

            if (JogMode == RCXJogMode.Joint)
            {
                var distances = (await _robotManagerAPI_V2.GetJogStepAnglesAsync(type) ?? new Dictionary<RCXJogJointAxis, double>());
                for (var i = 0; i < target.Length; i++)
                {
                    var axis = (RCXJogJointAxis)i;
                    if (distances.TryGetValue(axis, out var distance))
                    {
                        target[i] = distance;
                    }
                }
            }
            else
            {
                var distances = (await _robotManagerAPI_V2.GetJogStepDistancesAsync(JogMode, type) ?? new Dictionary<RCXJogCartesianAxis, double>());
                for (var i = 0; i < target.Length; i++)
                {
                    var axis = (RCXJogCartesianAxis)i;
                    if (distances.TryGetValue(axis, out var distance))
                    {
                        target[i] = distance;
                    }
                }
            }
        }

        /// <summary>
        /// Load jog distance.
        /// </summary>
        public async Task Load()
        {
            await Load(RCXJogDistance.Long);
            await Load(RCXJogDistance.Middle);
            await Load(RCXJogDistance.Short);
        }

        /// <summary>
        /// Save jog distance.
        /// </summary>
        public async Task Save()
        {
            Func<RCXJogDistance, double[], Task> save = async (type, target) =>
            {
                if (JogMode == RCXJogMode.Joint)
                {
                    var distances = new Dictionary<RCXJogJointAxis, double>();
                    
                    for (var i = 0; i < target.Length; i++)
                    {
                        distances[(RCXJogJointAxis)i] = target[i];
                    }

                    await _robotManagerAPI_V2.SetJogStepAnglesAsync(type, distances);
                }
                else
                {
                    var distances = new Dictionary<RCXJogCartesianAxis, double>();

                    for (var i = 0; i < target.Length; i++)
                    {
                        distances[(RCXJogCartesianAxis)i] = target[i];
                    }

                    await _robotManagerAPI_V2.SetJogStepDistancesAsync(JogMode, type, distances);
                }
            };

            await save(RCXJogDistance.Long, DistanceL);
            await save(RCXJogDistance.Middle, DistanceM);
            await save(RCXJogDistance.Short, DistanceS);
        }

        /// <summary>
        /// Get jog distance.
        /// </summary>
        /// <param name="jogD">Jog distance type</param>
        /// <param name="idx">Index</param>
        /// <returns></returns>
        public double GetDistance(RCXJogDistance jogDistance, int idx)
        {
            var ret = 0.0;

            if (jogDistance == RCXJogDistance.Long)
            {
                ret = DistanceL[idx];
            }

            if (jogDistance == RCXJogDistance.Middle)
            {
                ret = DistanceM[idx];
            }

            if (jogDistance == RCXJogDistance.Short)
            {
                ret = DistanceS[idx];
            }

            return ret;
        }

        /// <summary>
        /// Get jog distance.
        /// </summary>
        /// <param name="idx">Index</param>
        /// <returns></returns>
        public double GetDistance(int idx) => GetDistance(SelectedType, idx);
    }
}

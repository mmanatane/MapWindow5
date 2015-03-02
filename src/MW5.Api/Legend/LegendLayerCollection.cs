﻿using System;
using AxMapWinGIS;
using MW5.Api.Concrete;

namespace MW5.Api.Legend
{
    public class LegendLayerCollection : BaseLayerCollection<LegendLayer>
    {
        private readonly LegendControl _legend;

        internal LegendLayerCollection(AxMap axMap, LegendControl legend)
            : base(axMap)
        {
            _legend = legend;
            if (legend == null)
            {
                throw new NullReferenceException("Legend reference is null.");
            }
        }

        public override LegendLayer this[int position]
        {
            get
            {
                if (position >= 0 && position < Count)
                {
                    var layerHandle = _axMap.get_LayerHandle(position);
                    if (layerHandle != -1)
                    {
                        return new LegendLayer(_axMap, layerHandle, _legend);
                    }
                }

                return null;
            }
        }

        public override LegendLayer ItemByHandle(int layerHandle)
        {
            return new LegendLayer(_axMap, layerHandle, _legend);
        }

        /// <summary>
        /// Gets the position (index) of the specified layer within the group
        /// </summary>
        /// <param name="layerHandle">Handle of the layer</param>
        /// <returns>0-Based Index into list of layers within group, -1 on failure</returns>
        public int PositionInGroup(int layerHandle)
        {
            int layerIndex, groupIndex;

            var lyr = _legend.FindLayerByHandle(layerHandle, out groupIndex, out layerIndex);

            if (lyr != null)
            {
                return layerIndex;
            }

            return -1;
        }

        /// <summary>
        /// Gets the handle of the group containing the specified layer
        /// </summary>
        /// <param name="layerHandle">Handle of the layer</param>
        /// <returns>Group Handle of the group that contains the layer, -1 on failure</returns>
        public int GroupOf(int layerHandle)
        {
            int layerIndex, groupIndex;

            var lyr = _legend.FindLayerByHandle(layerHandle, out groupIndex, out layerIndex);

            if (lyr != null)
            {
                var grp = _legend.AllGroups[groupIndex];
                return grp.Handle;
            }

            return -1;
        }

        /// <summary>
        /// Move a layer to a specified location within a specified group
        /// </summary>
        /// <param name="layerHandle">Handle to the layer to move</param>
        /// <param name="targetGroupHandle">Handle of the group into which to move the layer</param>
        /// <param name="positionInGroup">0-Based index into the list of layers within the Target Group</param>
        /// <returns>True on success, False otherwise</returns>
        public bool MoveLayer(int layerHandle, int targetGroupHandle, int positionInGroup)
        {
            return _legend.MoveLayer(targetGroupHandle, layerHandle, positionInGroup);
        }
    }
}
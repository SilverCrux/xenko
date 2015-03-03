﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox.EntityModel
{

    /// <summary>
    /// The base class for scene editor's gizmos.
    /// </summary>
    public abstract class GizmoBase : ComponentBase, IGizmo
    {
        private bool isEnabled = true;
        private readonly EntityGroup entityGroup;
        private IGraphicsDeviceService graphicsDeviceService;

        /// <summary>
        /// The default entity group of the gizmo.
        /// </summary>
        public const EntityGroup DefaultGroup = EntityGroup.Group1;
        
        /// <summary>
        /// Gets the scene instance in charge of the gizmos.
        /// </summary>
        protected SceneInstance SceneInstance { get; private set; }

        /// <summary>
        /// Gets a registry containing the services available for the gizmos.
        /// </summary>
        protected IServiceRegistry Services { get { return SceneInstance.Services; } }

        /// <summary>
        /// Gets a reference to the input manager.
        /// </summary>
        protected InputManager Input { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        protected GraphicsDevice GraphicsDevice
        {
            get
            {
                if (graphicsDeviceService == null)
                {
                    throw new InvalidOperationException("GraphicsDeviceService is not yet initialized");
                }

                return graphicsDeviceService.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets the root entity of the gizmo.
        /// </summary>
        protected Entity RootEntity { get; private set; }

        public Entity SceneEntity { get; private set; }

        public virtual bool IsSelected { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="GizmoBase"/>.
        /// </summary>
        /// <param name="sceneEntity">The entity of the scene associated to the gizmo</param>
        /// <param name="group">The entity group of the gizmo</param>
        protected GizmoBase(Entity sceneEntity, EntityGroup group = DefaultGroup)
        {
            entityGroup = group;
            SceneEntity = sceneEntity;
        }

        public virtual bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;

                if (RootEntity == null)
                    return;

                if (value)
                    SceneInstance.Enable(RootEntity);
                else
                    SceneInstance.Disable(RootEntity);
            }
        }

        public abstract bool IsUnderMouse(GizmoContext context);

        public virtual void Initialize(SceneInstance sceneInstance)
        {
            SceneInstance = sceneInstance;
            Input = Services.GetServiceAs<InputManager>();
            graphicsDeviceService = Services.GetServiceAs<IGraphicsDeviceService>();
            RootEntity = Create();
            AssignEntityGroupToEntity(RootEntity);
            sceneInstance.Scene.AddChild(RootEntity);
            IsSelected = false;
        }

        protected override void Destroy()
        {
            base.Destroy();

            SceneInstance.Scene.RemoveChild(RootEntity);
        }

        /// <summary>
        /// Create the gizmo entity hierarchy.
        /// </summary>
        /// <returns>the gizmo root entity</returns>
        protected abstract Entity Create();

        private void AssignEntityGroupToEntity(Entity rootEntity)
        {
            rootEntity.Group = entityGroup;
            foreach (var subTransform in rootEntity.Transform.Children)
            {
                AssignEntityGroupToEntity(subTransform.Entity);
            }
        }

        public virtual void Update(GizmoContext context)
        {
            if (SceneEntity == null)
                return;

            var scale = context.SceneUnit;
            var transformMatrix = SceneEntity.Transform.WorldMatrix;
            transformMatrix.Row1 *= scale / transformMatrix.Row1.Length();
            transformMatrix.Row2 *= scale / transformMatrix.Row2.Length();
            transformMatrix.Row3 *= scale / transformMatrix.Row3.Length();
            RootEntity.Transform.LocalMatrix = transformMatrix;
            RootEntity.Transform.UseTRS = false;
        }

        public void PrepareDraw(RenderContext context)
        {
        }
    }
}
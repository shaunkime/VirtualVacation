using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DepthMergeEffect
{
    public class DepthMerge : ShaderEffect
    {
        #region Constructors

        static DepthMerge()
        {
            _pixelShader.UriSource = Global.MakePackUri("DepthMerge.ps");
        }

        public DepthMerge()
        {
            this.PixelShader = _pixelShader;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(MaskedActorProperty);
            UpdateShaderValue(ActorDepthProperty);
            UpdateShaderValue(MaskedActorProperty);
            UpdateShaderValue(BackgroundDepthProperty);
            UpdateShaderValue(ActorXOffsetProperty);
            UpdateShaderValue(ActorYOffsetProperty);
            UpdateShaderValue(ActorScaleProperty);
        }

        #endregion

        #region Dependency Properties

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(DepthMerge), 0);



        public Brush MaskedActor
        {
            get { return (Brush)GetValue(MaskedActorProperty); }
            set { SetValue(MaskedActorProperty, value); }
        }

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty MaskedActorProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("MaskedActor", typeof(DepthMerge), 1, SamplingMode.Bilinear);

        public Brush ActorDepth
        {
            get { return (Brush)GetValue(ActorDepthProperty); }
            set { SetValue(ActorDepthProperty, value); }
        }

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty ActorDepthProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("ActorDepth", typeof(DepthMerge), 2);


        public Brush BackgroundDepth
        {
            get { return (Brush)GetValue(BackgroundDepthProperty); }
            set { SetValue(BackgroundDepthProperty, value); }
        }

        // Brush-valued properties turn into sampler-property in the shader.
        // This helper sets "ImplicitInput" as the default, meaning the default
        // sampler is whatever the rendering of the element it's being applied to is.
        public static readonly DependencyProperty BackgroundDepthProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("BackgroundDepth", typeof(DepthMerge), 3);



        public double ActorXOffset
        {
            get { return (double)GetValue(ActorXOffsetProperty); }
            set { SetValue(ActorXOffsetProperty, value); }
        }

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty ActorXOffsetProperty =
            DependencyProperty.Register("ActorXOffset", typeof(double), typeof(DepthMerge),
                    new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));


        public double ActorYOffset
        {
            get { return (double)GetValue(ActorYOffsetProperty); }
            set { SetValue(ActorYOffsetProperty, value); }
        }

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty ActorYOffsetProperty =
            DependencyProperty.Register("ActorYOffset", typeof(double), typeof(DepthMerge),
                    new UIPropertyMetadata(0.0, PixelShaderConstantCallback(2)));



        public double ActorScale
        {
            get { return (double)GetValue(ActorScaleProperty); }
            set { SetValue(ActorScaleProperty, value); }
        }

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty ActorScaleProperty =
            DependencyProperty.Register("ActorScale", typeof(double), typeof(DepthMerge),
                    new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));


        #endregion

        #region Member Data

        private static PixelShader _pixelShader = new PixelShader();

        #endregion

    }
}

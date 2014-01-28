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
            this.DdxUvDdyUvRegisterIndex = 6;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(MaskedActorProperty);
            UpdateShaderValue(DisplacementProperty);
            UpdateShaderValue(AdditionalMaskedActorOpacityProperty);
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


        public double Displacement
        {
            get { return (double)GetValue(DisplacementProperty); }
            set { SetValue(DisplacementProperty, value); }
        }

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty DisplacementProperty =
            DependencyProperty.Register("Displacement", typeof(double), typeof(DepthMerge),
                    new UIPropertyMetadata(5.0, PixelShaderConstantCallback(0)));




        public double AdditionalMaskedActorOpacity
        {
            get { return (double)GetValue(AdditionalMaskedActorOpacityProperty); }
            set { SetValue(AdditionalMaskedActorOpacityProperty, value); }
        }

        // Scalar-valued properties turn into shader constants with the register
        // number sent into PixelShaderConstantCallback().
        public static readonly DependencyProperty AdditionalMaskedActorOpacityProperty =
            DependencyProperty.Register("AdditionalMaskedActorOpacity", typeof(double), typeof(DepthMerge),
                    new UIPropertyMetadata(5.0, PixelShaderConstantCallback(1)));


        #endregion

        #region Member Data

        private static PixelShader _pixelShader = new PixelShader();

        #endregion

    }
}

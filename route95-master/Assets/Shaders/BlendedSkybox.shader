Shader "Skybox/BlendedSkybox" {
   Properties {
      _Tint ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
      _Value ("Blend Value", Range (0.0, 1.0)) = 0.0
      _Front1 ("Front1", 2D) = "white" {}
      _Back1 ("Back1", 2D) = "white" {}
      _Left1 ("Left1", 2D) = "white" {}
      _Right1 ("Right1", 2D) = "white" {}
      _Up1 ("Up1", 2D) = "white" {}
      _Down1 ("Down1", 2D) = "white" {}
      _Front2 ("Front2", 2D) = "black" {}
      _Back2 ("Back2", 2D) = "black" {}
      _Left2 ("Left2", 2D) = "black" {}
      _Right2 ("Right2", 2D) = "black" {}
      _Up2 ("Up2", 2D) = "black" {}
      _Down2 ("Down2", 2D) = "black" {}
   }

   SubShader {
      Tags { "Queue"="Background"  }

      Cull Off 
      Fog { Mode Off }
      Lighting Off
      Color [_Tint]

      Pass {
      	SetTexture [_Front1] { combine texture }
      	SetTexture [_Front2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Front2] { constantColor (0, 0, 0, [_Value]) combine texture * previous }
      	//SetTexture [_Front2] { combine previous +- primary, previous * primary }
      }
      Pass {
      	SetTexture [_Back1] { combine texture }
      	SetTexture [_Back2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Back2] { combine previous +- primary, previous * primary }
      }
      Pass {
      	SetTexture [_Left1] { combine texture }
      	SetTexture [_Left2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Left2] { combine previous +- primary, previous * primary }
      }
      Pass {
      	SetTexture [_Right1] { combine texture }
      	SetTexture [_Right2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Right2] { combine previous +- primary, previous * primary }
      }
      Pass {
      	SetTexture [_Up1] { combine texture }
      	SetTexture [_Up2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Up2] { combine previous +- primary, previous * primary }
      }
      Pass {
      	SetTexture [_Down1] { combine texture }
      	SetTexture [_Down2] { constantColor (0, 0, 0, [_Value]) combine texture lerp(constant) previous }
      	//SetTexture [_Down2] { combine previous +- primary, previous * primary }
      }


   } 	
   Fallback "Skybox/6 Sided", 1
}
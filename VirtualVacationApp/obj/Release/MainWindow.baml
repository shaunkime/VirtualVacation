   M S B A M L   `   `   `  ���� S  OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=nulleHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.ToolkitMicrosoft.Kinect.Toolkit  K GDepthMergeEffect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM8clr-namespace:DepthMergeEffect;assembly=DepthMergeEffectDepthMergeEffect  VirtualVacation9   3Microsoft.Samples.Kinect.VirtualVacation.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     VkHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.Toolkit     Hdme8clr-namespace:DepthMergeEffect;assembly=DepthMergeEffect     -     � Title$  Virtual Vacation��$
��1000��$
��1920�� � 	Resources    liveDepthMap �� RelativeTransform �� CenterX �� CenterY  liveColorMap  backgroundDepthMap  MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  SnapshotButton	 �� 
TargetType
 �� Orientation ��Name �� TextAlignment ��Name �� Triggers P� Property P� Value �� 
TargetName  SquareCheckBox �Name��%O  &	        &	 "     &	 D     &	 f     &	 {     &	 �     '�� �     &	 �     7  7  &	 �    7  7  ��  �� $	 0.5Q�$	 0.5Q���  �� $	 0.5Q�$	 0.5Q���  �� $	 0.5Q�$	 0.5Q��� $ 	#ff6e6e6e���� $ 	#ff52318f���� $ 	#ff00BCF2���� " ��.���  �@J�SnapsToDevicePixels . �� " ��.���  � �� �� "	 ��.R�� .+��� $
 
Horizontal=�
������ .+��� $ SnapText��#��z��$
 Left=�$��Center=�8��  $��15(�� $��9,0,0,0q�.+��� $ 
SnapNormal��$��Images\SnapNormal.png��$
��None=�$��Center=��� $ 	SnapHover��$��Images\SnapHover.png��$
��None=�$��Center=�$y�	Collapsed=� P�  �@J�IsMouseOver . . ��� $ 
SnapNormal�� � y�$ 	Collapsed=��� $ 	SnapHover�� � y�$ Visible=��� 8  $ SnapText�� � ���� " ��.���  � �� �� "	 ��.R�� .+��� $
 
Horizontal=�
������ .+�� $ SquareCheckBoxChecked��.+��� $ CheckedNormal��$��Images\CheckedNormal.png��$
��None=�$��Center=��� $ CheckedHover��$��Images\CheckedHover.png��$
��None=�$��Center=�$y�	Collapsed=�� $ SquareCheckBoxUnchecked��$y�	Collapsed=�.+��� $ UncheckedNormal��$ ��Images\UncheckedNormal.png��$
��None=�$��Center=��� $ UncheckedHover��$��Images\UncheckedHover.png��$
��None=�$��Center=�$y�	Collapsed=��� $ SquareCheckBoxText��#��z��$
 Left=�$��Center=�8��  $��15(�$��9,0,0,0q� P�  �@d�	IsChecked$ falseY�. ��� $ SquareCheckBoxChecked�� � y�$ 	Collapsed=��� $ SquareCheckBoxUnchecked�� � y�$ Visible=�P�  �@J�IsMouseOver . . ��� $ CheckedNormal�� � y�$ 	Collapsed=��� $ UncheckedNormal�� � y�$ 	Collapsed=��� $ CheckedHover�� � y�$ Visible=��� $ UncheckedHover�� � y�$ Visible=��� 8  $ SquareCheckBoxText�� � ��.��� -   $��
layoutGrid��$��	10 0 10 0q� � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $��0��$��0 0 0 20q� � ColumnDefinitions �� $��* ��� $��* ��� $��* �.+��� $��0��$��Images\Logo.png��$
��Fill=�$��32��$��81��$
��Left=�$��0 10 0 5q��� $��2��$��0 0 -1 0q�$��Right=�$��Bottom=�#��[ $��Segoe UI)�$��18(�Virtual Vacation4   .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  -   $��sensorChooserUi��$��1��$��Center=�$��0 0 0 5q�$	��Top=�� $��1��.+��� -   $��Backdrop��$��Images/Background.jpg��$
��None=� J� Effect !  DepthMergeEffect.DepthMerge     MaskedActor# [    
ActorDepth# [     BackgroundDepth# [    
ActorScale ��  �� ElementName$ actorScaleSlider�� �� Path$ Value�   ActorXOffset �� $ actorXOffsetSlider��$ Value�   ActorYOffset �� $ actorYOffsetSlider��$ Value�� $��2��.+��� .+��� $
 
Horizontal=�.+��� .��
ActorScale�� -   $��actorScaleSlider��$��25��$
��Left=� � Minimum$ 0Q� � Maximum$ 10Q�  � SmallChange$  .1Q�! � LargeChange$! 1Q�" � Value$" 1Q�$��Bottom=�$	��289���� $��28��$
��Left=�$��Bottom=�$	��120������ $ actorScaleSlider��$ Value��� $
 
Horizontal=�.+��� .��Actor X Offset�� -   $��actorXOffsetSlider��$��25��$
��Left=�$
 -5.0Q�$	 5.0Q�$
  0.05Q�$
! 0.05Q�$	" 0.0Q�$��Bottom=�$	��289���� $��28��$
��Left=�$��Bottom=�$	��120������ $ actorXOffsetSlider��$ Value��� $
 
Horizontal=�.+��� .��Actor Y Offset�� -   $��actorYOffsetSlider��$��25��$
��Left=�$
 -5.0Q�$	 5.0Q�$
  0.05Q�$
! 0.05Q�$	" 0.0Q�$��Bottom=�$	��289���� $��28��$
��Left=�$��Bottom=�$	��120������ $ actorYOffsetSlider��$ Value��� -   $��buttonScreenshot��$��3��#��[ $��
Screenshot��$
��Auto��$��Right=�$��Center=�$��
10 10 0 10q��� -	   $��checkBoxNearMode��$��3��#��[ $��	Near Mode��$
��Auto��$
��Left=�$��Center=�$��
0 10 10 10q��� -
   $��	statusBar��$��4��$��Stretch=�$��Bottom=�
�������#��[ .@��� $��0 0 0 10q�.���� -   $��statusBarText��$��-1 0 0 0q�JHPress 'Screenshot' to save a screenshot to your 'My Pictures' directory.
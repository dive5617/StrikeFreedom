﻿#pragma checksum "..\..\..\Model_Window_DeNovo.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F85059076769769082C6D21BE136F2F3"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18063
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using OxyPlot.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace pBuild {
    
    
    /// <summary>
    /// Model_Window_DeNovo
    /// </summary>
    public partial class Model_Window_DeNovo : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\Model_Window_DeNovo.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Model_Window_DeNovo.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Model_Window_DeNovo.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal OxyPlot.Wpf.Plot model;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/pBuild;component/model_window_denovo.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Model_Window_DeNovo.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 5 "..\..\..\Model_Window_DeNovo.xaml"
            ((pBuild.Model_Window_DeNovo)(target)).Initialized += new System.EventHandler(this.Window_Initialized);
            
            #line default
            #line hidden
            
            #line 5 "..\..\..\Model_Window_DeNovo.xaml"
            ((pBuild.Model_Window_DeNovo)(target)).ContentRendered += new System.EventHandler(this.Window_ContentRendered);
            
            #line default
            #line hidden
            
            #line 5 "..\..\..\Model_Window_DeNovo.xaml"
            ((pBuild.Model_Window_DeNovo)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.ctrl_C_keydown);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 12 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.initial_clk);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 13 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.clear_btn_clk);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 14 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.end_btn_clk);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 15 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.back_btn_clk);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 16 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.setting_btn_clk);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 17 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.config_btn_clk);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 18 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.copy_to_clipboard_btn_clk);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 19 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Click += new System.Windows.RoutedEventHandler(this.deisotope_btn_clk);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 20 "..\..\..\Model_Window_DeNovo.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.pNovo_btn_clk);
            
            #line default
            #line hidden
            return;
            case 11:
            this.grid = ((System.Windows.Controls.Grid)(target));
            return;
            case 12:
            this.border = ((System.Windows.Controls.Border)(target));
            return;
            case 13:
            this.model = ((OxyPlot.Wpf.Plot)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


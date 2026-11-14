; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [31 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [93 x i64] [
	i64 u0x02abedc11addc1ed, ; 0: lib_Mono.Android.Runtime.dll.so => 29
	i64 u0x0581db89237110e9, ; 1: lib_System.Collections.dll.so => 6
	i64 u0x09d144a7e214d457, ; 2: System.Security.Cryptography => 22
	i64 u0x0c59ad9fbbd43abe, ; 3: Mono.Android => 30
	i64 u0x13f1e5e209e91af4, ; 4: lib_Java.Interop.dll.so => 28
	i64 u0x17f9358913beb16a, ; 5: System.Text.Encodings.Web => 23
	i64 u0x1a91866a319e9259, ; 6: lib_System.Collections.Concurrent.dll.so => 5
	i64 u0x1c753b5ff15bce1b, ; 7: Mono.Android.Runtime.dll => 29
	i64 u0x2174319c0d835bc9, ; 8: System.Runtime => 21
	i64 u0x21cd54969f5c07d1, ; 9: lib_LazerArena.Android.dll.so => 4
	i64 u0x2407aef2bbe8fadf, ; 10: System.Console => 9
	i64 u0x247619fe4413f8bf, ; 11: System.Runtime.Serialization.Primitives.dll => 20
	i64 u0x277a71da005630e0, ; 12: LazerArena.Android => 4
	i64 u0x27b410442fad6cf1, ; 13: Java.Interop.dll => 28
	i64 u0x2af298f63581d886, ; 14: System.Text.RegularExpressions.dll => 25
	i64 u0x2c37bd102c76a029, ; 15: lib_LazerArena.Core.dll.so => 2
	i64 u0x2d169d318a968379, ; 16: System.Threading.dll => 26
	i64 u0x2db915caf23548d2, ; 17: System.Text.Json.dll => 24
	i64 u0x31195fef5d8fb552, ; 18: _Microsoft.Android.Resource.Designer.dll => 0
	i64 u0x3235427f8d12dae1, ; 19: lib_System.Drawing.Primitives.dll.so => 10
	i64 u0x434c4e1d9284cdae, ; 20: Mono.Android.dll => 30
	i64 u0x4499fa3c8e494654, ; 21: lib_System.Runtime.Serialization.Primitives.dll.so => 20
	i64 u0x490cffb50a3cc73e, ; 22: MonoGame.Framework => 1
	i64 u0x49e952f19a4e2022, ; 23: System.ObjectModel => 17
	i64 u0x4b7b6532ded934b7, ; 24: System.Text.Json => 24
	i64 u0x4e32f00cb0937401, ; 25: Mono.Android.Runtime => 29
	i64 u0x5047a57f6df94029, ; 26: LazerArena.Presentation.dll => 3
	i64 u0x50e969a0f2075bd7, ; 27: LazerArena.Core.dll => 2
	i64 u0x51bb8a2afe774e32, ; 28: System.Drawing => 11
	i64 u0x54795225dd1587af, ; 29: lib_System.Runtime.dll.so => 21
	i64 u0x571c5cfbec5ae8e2, ; 30: System.Private.Uri => 18
	i64 u0x579a06fed6eec900, ; 31: System.Private.CoreLib.dll => 27
	i64 u0x5a8f6699f4a1caa9, ; 32: lib_System.Threading.dll.so => 26
	i64 u0x5ae9cd33b15841bf, ; 33: System.ComponentModel => 8
	i64 u0x5db0cbbd1028510e, ; 34: lib_System.Runtime.InteropServices.dll.so => 19
	i64 u0x5ea92fdb19ec8c4c, ; 35: System.Text.Encodings.Web.dll => 23
	i64 u0x60f62d786afcf130, ; 36: System.Memory => 15
	i64 u0x622eef6f9e59068d, ; 37: System.Private.CoreLib => 27
	i64 u0x644c62132b2c3ab3, ; 38: LazerArena.Presentation => 3
	i64 u0x6692e924eade1b29, ; 39: lib_System.Console.dll.so => 9
	i64 u0x6872ec7a2e36b1ac, ; 40: System.Drawing.Primitives.dll => 10
	i64 u0x6a4d7577b2317255, ; 41: System.Runtime.InteropServices.dll => 19
	i64 u0x73e4ce94e2eb6ffc, ; 42: lib_System.Memory.dll.so => 15
	i64 u0x7a9f478b0bfe88fc, ; 43: lib_LazerArena.Presentation.dll.so => 3
	i64 u0x7bef86a4335c4870, ; 44: System.ComponentModel.TypeConverter => 7
	i64 u0x7d8ee2bdc8e3aad1, ; 45: System.Numerics.Vectors => 16
	i64 u0x7dfc3d6d9d8d7b70, ; 46: System.Collections => 6
	i64 u0x7e946809d6008ef2, ; 47: lib_System.ObjectModel.dll.so => 17
	i64 u0x7ecc13347c8fd849, ; 48: lib_System.ComponentModel.dll.so => 8
	i64 u0x82df8f5532a10c59, ; 49: lib_System.Drawing.dll.so => 11
	i64 u0x8d7b8ab4b3310ead, ; 50: System.Threading => 26
	i64 u0x8da188285aadfe8e, ; 51: System.Collections.Concurrent => 5
	i64 u0x903101b46fb73a04, ; 52: _Microsoft.Android.Resource.Designer => 0
	i64 u0x90393bd4865292f3, ; 53: lib_System.IO.Compression.dll.so => 12
	i64 u0x90da922f4ccbc487, ; 54: lib_MonoGame.Framework.dll.so => 1
	i64 u0x9157bd523cd7ed36, ; 55: lib_System.Text.Json.dll.so => 24
	i64 u0x91a74f07b30d37e2, ; 56: System.Linq.dll => 14
	i64 u0x944077d8ca3c6580, ; 57: System.IO.Compression.dll => 12
	i64 u0x97b8c771ea3e4220, ; 58: System.ComponentModel.dll => 8
	i64 u0x97e144c9d3c6976e, ; 59: System.Collections.Concurrent.dll => 5
	i64 u0xa0d8259f4cc284ec, ; 60: lib_System.Security.Cryptography.dll.so => 22
	i64 u0xa2572680829d2c7c, ; 61: System.IO.Pipelines.dll => 13
	i64 u0xa5e599d1e0524750, ; 62: System.Numerics.Vectors.dll => 16
	i64 u0xa5f1ba49b85dd355, ; 63: System.Security.Cryptography.dll => 22
	i64 u0xadc90ab061a9e6e4, ; 64: System.ComponentModel.TypeConverter.dll => 7
	i64 u0xae282bcd03739de7, ; 65: Java.Interop => 28
	i64 u0xae53579c90db1107, ; 66: System.ObjectModel.dll => 17
	i64 u0xb220631954820169, ; 67: System.Text.RegularExpressions => 25
	i64 u0xb4bd7015ecee9d86, ; 68: System.IO.Pipelines => 13
	i64 u0xb7212c4683a94afe, ; 69: System.Drawing.Primitives => 10
	i64 u0xb81a2c6e0aee50fe, ; 70: lib_System.Private.CoreLib.dll.so => 27
	i64 u0xba48785529705af9, ; 71: System.Collections.dll => 6
	i64 u0xc0d928351ab5ca77, ; 72: System.Console.dll => 9
	i64 u0xc12b8b3afa48329c, ; 73: lib_System.Linq.dll.so => 14
	i64 u0xc19c683f8c886643, ; 74: LazerArena.Core => 2
	i64 u0xc50fded0ded1418c, ; 75: lib_System.ComponentModel.TypeConverter.dll.so => 7
	i64 u0xc5a0f4b95a699af7, ; 76: lib_System.Private.Uri.dll.so => 18
	i64 u0xcbd4fdd9cef4a294, ; 77: lib__Microsoft.Android.Resource.Designer.dll.so => 0
	i64 u0xcc2876b32ef2794c, ; 78: lib_System.Text.RegularExpressions.dll.so => 25
	i64 u0xcc667334debe5bbf, ; 79: MonoGame.Framework.dll => 1
	i64 u0xcebbd2b7d1b9d60d, ; 80: LazerArena.Android.dll => 4
	i64 u0xd333d0af9e423810, ; 81: System.Runtime.InteropServices => 19
	i64 u0xd3651b6fc3125825, ; 82: System.Private.Uri.dll => 18
	i64 u0xdbf9607a441b4505, ; 83: System.Linq => 14
	i64 u0xdd2b722d78ef5f43, ; 84: System.Runtime.dll => 21
	i64 u0xdd67031857c72f96, ; 85: lib_System.Text.Encodings.Web.dll.so => 23
	i64 u0xe192a588d4410686, ; 86: lib_System.IO.Pipelines.dll.so => 13
	i64 u0xe4f74a0b5bf9703f, ; 87: System.Runtime.Serialization.Primitives => 20
	i64 u0xe5434e8a119ceb69, ; 88: lib_Mono.Android.dll.so => 30
	i64 u0xe89a2a9ef110899b, ; 89: System.Drawing.dll => 11
	i64 u0xedc632067fb20ff3, ; 90: System.Memory.dll => 15
	i64 u0xf4c1dd70a5496a17, ; 91: System.IO.Compression => 12
	i64 u0xf7e2cac4c45067b3 ; 92: lib_System.Numerics.Vectors.dll.so => 16
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [93 x i32] [
	i32 29, i32 6, i32 22, i32 30, i32 28, i32 23, i32 5, i32 29,
	i32 21, i32 4, i32 9, i32 20, i32 4, i32 28, i32 25, i32 2,
	i32 26, i32 24, i32 0, i32 10, i32 30, i32 20, i32 1, i32 17,
	i32 24, i32 29, i32 3, i32 2, i32 11, i32 21, i32 18, i32 27,
	i32 26, i32 8, i32 19, i32 23, i32 15, i32 27, i32 3, i32 9,
	i32 10, i32 19, i32 15, i32 3, i32 7, i32 16, i32 6, i32 17,
	i32 8, i32 11, i32 26, i32 5, i32 0, i32 12, i32 1, i32 24,
	i32 14, i32 12, i32 8, i32 5, i32 22, i32 13, i32 16, i32 22,
	i32 7, i32 28, i32 17, i32 25, i32 13, i32 10, i32 27, i32 6,
	i32 9, i32 14, i32 2, i32 7, i32 18, i32 0, i32 25, i32 1,
	i32 4, i32 19, i32 18, i32 14, i32 21, i32 23, i32 13, i32 20,
	i32 30, i32 11, i32 15, i32 12, i32 16
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 11, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [11 x %struct.MarshalMethodsManagedClass] [
	%struct.MarshalMethodsManagedClass {
		i32 u0x02000077, ; class name: Android.Media.MediaPlayer/IOnCompletionListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 0
	%struct.MarshalMethodsManagedClass {
		i32 u0x020000f8, ; class name: Java.IO.InputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 1
	%struct.MarshalMethodsManagedClass {
		i32 u0x020000fb, ; class name: Java.IO.OutputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 2
	%struct.MarshalMethodsManagedClass {
		i32 u0x0200010e, ; class name: Java.Lang.IRunnableInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 3
	%struct.MarshalMethodsManagedClass {
		i32 u0x020000eb, ; class name: Android.App.Activity, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 4
	%struct.MarshalMethodsManagedClass {
		i32 u0x0200007f, ; class name: Android.Views.View, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 5
	%struct.MarshalMethodsManagedClass {
		i32 u0x02000091, ; class name: Android.Views.ISurfaceHolderCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 6
	%struct.MarshalMethodsManagedClass {
		i32 u0x02000081, ; class name: Android.Views.View/IOnTouchListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 7
	%struct.MarshalMethodsManagedClass {
		i32 u0x0200009a, ; class name: Android.Views.OrientationEventListener, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 8
	%struct.MarshalMethodsManagedClass {
		i32 u0x020000d7, ; class name: Android.Content.BroadcastReceiver, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	}, ; 9
	%struct.MarshalMethodsManagedClass {
		i32 u0x0200011f, ; class name: Java.Interop.TypeManager/JavaTypeManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
		ptr null; MonoClass* klass
	} ; 10
], align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [11 x ptr] [
	ptr @.mm.0, ; 0 ('Android.Media.MediaPlayer/IOnCompletionListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.1, ; 1 ('Java.IO.InputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.2, ; 2 ('Java.IO.OutputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.3, ; 3 ('Java.Lang.IRunnableInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.4, ; 4 ('Android.App.Activity, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.5, ; 5 ('Android.Views.View, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.6, ; 6 ('Android.Views.ISurfaceHolderCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.7, ; 7 ('Android.Views.View/IOnTouchListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.8, ; 8 ('Android.Views.OrientationEventListener, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.9, ; 9 ('Android.Content.BroadcastReceiver, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
	ptr @.mm.10 ; 10 ('Java.Interop.TypeManager/JavaTypeManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')
], align 8

@mm_method_names = dso_local local_unnamed_addr constant [27 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000001e06000299, ; name: n_OnCompletion_Landroid_media_MediaPlayer__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.0_name; char* name
	}, ; 0
	%struct.MarshalMethodName {
		i64 u0x0000001e0600074f, ; name: n_Close_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.1_name; char* name
	}, ; 1
	%struct.MarshalMethodName {
		i64 u0x0000001e06000750, ; name: n_Read_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.2_name; char* name
	}, ; 2
	%struct.MarshalMethodName {
		i64 u0x0000001e06000751, ; name: n_Read_arrayB_mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.3_name; char* name
	}, ; 3
	%struct.MarshalMethodName {
		i64 u0x0000001e06000752, ; name: n_Read_arrayBII_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32)
		ptr @.MarshalMethodName.4_name; char* name
	}, ; 4
	%struct.MarshalMethodName {
		i64 u0x0000001e0600076f, ; name: n_Close_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.1_name; char* name
	}, ; 5
	%struct.MarshalMethodName {
		i64 u0x0000001e06000770, ; name: n_Flush_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.5_name; char* name
	}, ; 6
	%struct.MarshalMethodName {
		i64 u0x0000001e06000771, ; name: n_Write_arrayB_mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.6_name; char* name
	}, ; 7
	%struct.MarshalMethodName {
		i64 u0x0000001e06000772, ; name: n_Write_arrayBII_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32)
		ptr @.MarshalMethodName.7_name; char* name
	}, ; 8
	%struct.MarshalMethodName {
		i64 u0x0000001e06000773, ; name: n_Write_I_mm_wrapper(IntPtr,IntPtr,Int32)
		ptr @.MarshalMethodName.8_name; char* name
	}, ; 9
	%struct.MarshalMethodName {
		i64 u0x0000001e06000822, ; name: n_Run_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.9_name; char* name
	}, ; 10
	%struct.MarshalMethodName {
		i64 u0x0000001e060006e1, ; name: n_OnCreate_Landroid_os_Bundle__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.10_name; char* name
	}, ; 11
	%struct.MarshalMethodName {
		i64 u0x0000001e060006e2, ; name: n_OnConfigurationChanged_Landroid_content_res_Configuration__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.11_name; char* name
	}, ; 12
	%struct.MarshalMethodName {
		i64 u0x0000001e060006e3, ; name: n_OnPause_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.12_name; char* name
	}, ; 13
	%struct.MarshalMethodName {
		i64 u0x0000001e060006e4, ; name: n_OnResume_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.13_name; char* name
	}, ; 14
	%struct.MarshalMethodName {
		i64 u0x0000001e060006e5, ; name: n_OnDestroy_mm_wrapper(IntPtr,IntPtr)
		ptr @.MarshalMethodName.14_name; char* name
	}, ; 15
	%struct.MarshalMethodName {
		i64 u0x0000001e060002c1, ; name: n_OnKeyDown_ILandroid_view_KeyEvent__mm_wrapper(IntPtr,IntPtr,Int32,IntPtr)
		ptr @.MarshalMethodName.15_name; char* name
	}, ; 16
	%struct.MarshalMethodName {
		i64 u0x0000001e060002c2, ; name: n_OnKeyUp_ILandroid_view_KeyEvent__mm_wrapper(IntPtr,IntPtr,Int32,IntPtr)
		ptr @.MarshalMethodName.16_name; char* name
	}, ; 17
	%struct.MarshalMethodName {
		i64 u0x0000001e060002c3, ; name: n_OnGenericMotionEvent_Landroid_view_MotionEvent__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.17_name; char* name
	}, ; 18
	%struct.MarshalMethodName {
		i64 u0x0000001e06000348, ; name: n_SurfaceChanged_Landroid_view_SurfaceHolder_III_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32,Int32)
		ptr @.MarshalMethodName.18_name; char* name
	}, ; 19
	%struct.MarshalMethodName {
		i64 u0x0000001e06000349, ; name: n_SurfaceCreated_Landroid_view_SurfaceHolder__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.19_name; char* name
	}, ; 20
	%struct.MarshalMethodName {
		i64 u0x0000001e0600034a, ; name: n_SurfaceDestroyed_Landroid_view_SurfaceHolder__mm_wrapper(IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.20_name; char* name
	}, ; 21
	%struct.MarshalMethodName {
		i64 u0x0000001e060002ce, ; name: n_OnTouch_Landroid_view_View_Landroid_view_MotionEvent__mm_wrapper(IntPtr,IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.21_name; char* name
	}, ; 22
	%struct.MarshalMethodName {
		i64 u0x0000001e060003f1, ; name: n_OnOrientationChanged_I_mm_wrapper(IntPtr,IntPtr,Int32)
		ptr @.MarshalMethodName.22_name; char* name
	}, ; 23
	%struct.MarshalMethodName {
		i64 u0x0000001e06000624, ; name: n_OnReceive_Landroid_content_Context_Landroid_content_Intent__mm_wrapper(IntPtr,IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.23_name; char* name
	}, ; 24
	%struct.MarshalMethodName {
		i64 u0x0000001e06000868, ; name: n_Activate_mm(IntPtr,IntPtr,IntPtr,IntPtr,IntPtr,IntPtr)
		ptr @.MarshalMethodName.24_name; char* name
	}, ; 25
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.25_name; char* name
	} ; 26
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Marshal methods backing fields, pointers to native functions
@native_cb_onCompletion_0_0_6000299 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_close_0_1_600074f = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_read_0_1_6000750 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_read_0_1_6000751 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_read_0_1_6000752 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_close_0_2_600076f = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_flush_0_2_6000770 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_write_0_2_6000771 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_write_0_2_6000772 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_write_0_2_6000773 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_run_0_3_6000822 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onCreate_0_4_60006e1 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onConfigurationChanged_0_4_60006e2 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onPause_0_4_60006e3 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onResume_0_4_60006e4 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onDestroy_0_4_60006e5 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onKeyDown_0_5_60002c1 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onKeyUp_0_5_60002c2 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onGenericMotionEvent_0_5_60002c3 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_surfaceChanged_0_6_6000348 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_surfaceCreated_0_6_6000349 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_surfaceDestroyed_0_6_600034a = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onTouch_0_7_60002ce = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onOrientationChanged_0_8_60003f1 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_onReceive_0_9_6000624 = internal dso_local unnamed_addr global ptr null, align 8
@native_cb_activate_0_10_6000868 = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.mm.11)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Method: System.Void Android.Media.MediaPlayer/IOnCompletionListenerInvoker::n_OnCompletion_Landroid_media_MediaPlayer__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Media.MediaPlayer/IOnCompletionListener::OnCompletion(Android.Media.MediaPlayer)
; Implemented: System.Void Android.Media.MediaPlayer/IOnCompletionListener::OnCompletion(Android.Media.MediaPlayer)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_media_MediaPlayer_1OnCompletionListenerImplementor_n_1onCompletion(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onCompletion_0_0_6000299, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 0, i32 noundef 100663961, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onCompletion_0_0_6000299)
	%cb2 = load ptr, ptr @native_cb_onCompletion_0_0_6000299, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret void
}

; Method: System.Void Java.IO.InputStream::n_Close_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.InputStream::Close()
; Implemented: System.Void Android.Runtime.InputStreamAdapter::Close()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_InputStreamAdapter_n_1close(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_close_0_1_600074f, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 1, i32 noundef 100665167, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_close_0_1_600074f)
	%cb2 = load ptr, ptr @native_cb_close_0_1_600074f, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Int32 Java.IO.InputStream::n_Read_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Int32 Java.IO.InputStream::Read()
; Implemented: System.Int32 Android.Runtime.InputStreamAdapter::Read()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i32 @Java_mono_android_runtime_InputStreamAdapter_n_1read__(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_read_0_1_6000750, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 1, i32 noundef 100665168, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_read_0_1_6000750)
	%cb2 = load ptr, ptr @native_cb_read_0_1_6000750, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	%1 = tail call noundef i32 %fn(ptr noundef %env, ptr noundef %klass)
	ret i32 %1
}

; Method: System.Int32 Java.IO.InputStream::n_Read_arrayB_mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Int32 Java.IO.InputStream::Read(System.Byte[])
; Implemented: System.Int32 Android.Runtime.InputStreamAdapter::Read(System.Byte[])
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i32 @Java_mono_android_runtime_InputStreamAdapter_n_1read___3B(ptr noundef %env, ptr noundef %klass, ptr noundef %bytes) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_read_0_1_6000751, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 1, i32 noundef 100665169, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_read_0_1_6000751)
	%cb2 = load ptr, ptr @native_cb_read_0_1_6000751, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	%1 = tail call noundef i32 %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %bytes)
	ret i32 %1
}

; Method: System.Int32 Java.IO.InputStream::n_Read_arrayBII_mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr,System.Int32,System.Int32)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Int32 Java.IO.InputStream::Read(System.Byte[],System.Int32,System.Int32)
; Implemented: System.Int32 Android.Runtime.InputStreamAdapter::Read(System.Byte[],System.Int32,System.Int32)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i32 @Java_mono_android_runtime_InputStreamAdapter_n_1read___3BII(ptr noundef %env, ptr noundef %klass, ptr noundef %bytes, i32 noundef %offset, i32 noundef %length) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_read_0_1_6000752, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 1, i32 noundef 100665170, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_read_0_1_6000752)
	%cb2 = load ptr, ptr @native_cb_read_0_1_6000752, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	%1 = tail call noundef i32 %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %bytes, i32 noundef %offset, i32 noundef %length)
	ret i32 %1
}

; Method: System.Void Java.IO.OutputStream::n_Close_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.OutputStream::Close()
; Implemented: System.Void Android.Runtime.OutputStreamAdapter::Close()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_OutputStreamAdapter_n_1close(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_close_0_2_600076f, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 2, i32 noundef 100665199, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_close_0_2_600076f)
	%cb2 = load ptr, ptr @native_cb_close_0_2_600076f, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Void Java.IO.OutputStream::n_Flush_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.OutputStream::Flush()
; Implemented: System.Void Android.Runtime.OutputStreamAdapter::Flush()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_OutputStreamAdapter_n_1flush(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_flush_0_2_6000770, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 2, i32 noundef 100665200, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_flush_0_2_6000770)
	%cb2 = load ptr, ptr @native_cb_flush_0_2_6000770, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Void Java.IO.OutputStream::n_Write_arrayB_mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.OutputStream::Write(System.Byte[])
; Implemented: System.Void Android.Runtime.OutputStreamAdapter::Write(System.Byte[])
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_OutputStreamAdapter_n_1write___3B(ptr noundef %env, ptr noundef %klass, ptr noundef %buffer) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_write_0_2_6000771, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 2, i32 noundef 100665201, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_write_0_2_6000771)
	%cb2 = load ptr, ptr @native_cb_write_0_2_6000771, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %buffer)
	ret void
}

; Method: System.Void Java.IO.OutputStream::n_Write_arrayBII_mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr,System.Int32,System.Int32)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.OutputStream::Write(System.Byte[],System.Int32,System.Int32)
; Implemented: System.Void Android.Runtime.OutputStreamAdapter::Write(System.Byte[],System.Int32,System.Int32)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_OutputStreamAdapter_n_1write___3BII(ptr noundef %env, ptr noundef %klass, ptr noundef %buffer, i32 noundef %offset, i32 noundef %length) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_write_0_2_6000772, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 2, i32 noundef 100665202, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_write_0_2_6000772)
	%cb2 = load ptr, ptr @native_cb_write_0_2_6000772, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %buffer, i32 noundef %offset, i32 noundef %length)
	ret void
}

; Method: System.Void Java.IO.OutputStream::n_Write_I_mm_wrapper(System.IntPtr,System.IntPtr,System.Int32)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.IO.OutputStream::Write(System.Int32)
; Implemented: System.Void Android.Runtime.OutputStreamAdapter::Write(System.Int32)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_runtime_OutputStreamAdapter_n_1write__I(ptr noundef %env, ptr noundef %klass, i32 noundef %oneByte) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_write_0_2_6000773, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 2, i32 noundef 100665203, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_write_0_2_6000773)
	%cb2 = load ptr, ptr @native_cb_write_0_2_6000773, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, i32 noundef %oneByte)
	ret void
}

; Method: System.Void Java.Lang.IRunnableInvoker::n_Run_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Java.Lang.IRunnable::Run()
; Implemented: System.Void Java.Lang.IRunnable::Run()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_java_lang_RunnableImplementor_n_1run(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_run_0_3_6000822, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 3, i32 noundef 100665378, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_run_0_3_6000822)
	%cb2 = load ptr, ptr @native_cb_run_0_3_6000822, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnCreate_Landroid_os_Bundle__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnCreate(Android.OS.Bundle)
; Implemented: System.Void Microsoft.Xna.Framework.AndroidGameActivity::OnCreate(Android.OS.Bundle)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_AndroidGameActivity_n_1onCreate__Landroid_os_Bundle_2(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onCreate_0_4_60006e1, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665057, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onCreate_0_4_60006e1)
	%cb2 = load ptr, ptr @native_cb_onCreate_0_4_60006e1, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnCreate_Landroid_os_Bundle__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnCreate(Android.OS.Bundle)
; Implemented: System.Void LazerArena.Android.Activity1::OnCreate(Android.OS.Bundle)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64877c2053c119ecaa_Activity1_n_1onCreate__Landroid_os_Bundle_2(ptr noundef %env, ptr noundef %klass, ptr noundef %bundle) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onCreate_0_4_60006e1, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665057, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onCreate_0_4_60006e1)
	%cb2 = load ptr, ptr @native_cb_onCreate_0_4_60006e1, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %bundle)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnConfigurationChanged_Landroid_content_res_Configuration__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnConfigurationChanged(Android.Content.Res.Configuration)
; Implemented: System.Void Microsoft.Xna.Framework.AndroidGameActivity::OnConfigurationChanged(Android.Content.Res.Configuration)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_AndroidGameActivity_n_1onConfigurationChanged(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onConfigurationChanged_0_4_60006e2, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665058, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onConfigurationChanged_0_4_60006e2)
	%cb2 = load ptr, ptr @native_cb_onConfigurationChanged_0_4_60006e2, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnPause_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnPause()
; Implemented: System.Void Microsoft.Xna.Framework.AndroidGameActivity::OnPause()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_AndroidGameActivity_n_1onPause(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onPause_0_4_60006e3, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665059, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onPause_0_4_60006e3)
	%cb2 = load ptr, ptr @native_cb_onPause_0_4_60006e3, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnResume_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnResume()
; Implemented: System.Void Microsoft.Xna.Framework.AndroidGameActivity::OnResume()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_AndroidGameActivity_n_1onResume(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onResume_0_4_60006e4, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665060, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onResume_0_4_60006e4)
	%cb2 = load ptr, ptr @native_cb_onResume_0_4_60006e4, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.Void Android.App.Activity::n_OnDestroy_mm_wrapper(System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.App.Activity::OnDestroy()
; Implemented: System.Void Microsoft.Xna.Framework.AndroidGameActivity::OnDestroy()
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_AndroidGameActivity_n_1onDestroy(ptr noundef %env, ptr noundef %klass) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onDestroy_0_4_60006e5, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 4, i32 noundef 100665061, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onDestroy_0_4_60006e5)
	%cb2 = load ptr, ptr @native_cb_onDestroy_0_4_60006e5, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass)
	ret void
}

; Method: System.SByte Android.Views.View::n_OnKeyDown_ILandroid_view_KeyEvent__mm_wrapper(System.IntPtr,System.IntPtr,System.Int32,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Boolean Android.Views.View::OnKeyDown(Android.Views.Keycode,Android.Views.KeyEvent)
; Implemented: System.Boolean Microsoft.Xna.Framework.MonoGameAndroidGameView::OnKeyDown(Android.Views.Keycode,Android.Views.KeyEvent)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i1 @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1onKeyDown(ptr noundef %env, ptr noundef %klass, i32 noundef %0, ptr noundef %1) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onKeyDown_0_5_60002c1, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %2
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 5, i32 noundef 100664001, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onKeyDown_0_5_60002c1)
	%cb2 = load ptr, ptr @native_cb_onKeyDown_0_5_60002c1, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %2
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %2]
	%3 = tail call noundef i1 %fn(ptr noundef %env, ptr noundef %klass, i32 noundef %0, ptr noundef %1)
	ret i1 %3
}

; Method: System.SByte Android.Views.View::n_OnKeyUp_ILandroid_view_KeyEvent__mm_wrapper(System.IntPtr,System.IntPtr,System.Int32,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Boolean Android.Views.View::OnKeyUp(Android.Views.Keycode,Android.Views.KeyEvent)
; Implemented: System.Boolean Microsoft.Xna.Framework.MonoGameAndroidGameView::OnKeyUp(Android.Views.Keycode,Android.Views.KeyEvent)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i1 @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1onKeyUp(ptr noundef %env, ptr noundef %klass, i32 noundef %0, ptr noundef %1) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onKeyUp_0_5_60002c2, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %2
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 5, i32 noundef 100664002, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onKeyUp_0_5_60002c2)
	%cb2 = load ptr, ptr @native_cb_onKeyUp_0_5_60002c2, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %2
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %2]
	%3 = tail call noundef i1 %fn(ptr noundef %env, ptr noundef %klass, i32 noundef %0, ptr noundef %1)
	ret i1 %3
}

; Method: System.SByte Android.Views.View::n_OnGenericMotionEvent_Landroid_view_MotionEvent__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Boolean Android.Views.View::OnGenericMotionEvent(Android.Views.MotionEvent)
; Implemented: System.Boolean Microsoft.Xna.Framework.MonoGameAndroidGameView::OnGenericMotionEvent(Android.Views.MotionEvent)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i1 @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1onGenericMotionEvent(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onGenericMotionEvent_0_5_60002c3, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 5, i32 noundef 100664003, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onGenericMotionEvent_0_5_60002c3)
	%cb2 = load ptr, ptr @native_cb_onGenericMotionEvent_0_5_60002c3, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	%2 = tail call noundef i1 %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret i1 %2
}

; Method: System.Void Android.Views.ISurfaceHolderCallbackInvoker::n_SurfaceChanged_Landroid_view_SurfaceHolder_III_mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr,System.Int32,System.Int32,System.Int32)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Views.ISurfaceHolderCallback::SurfaceChanged(Android.Views.ISurfaceHolder,Android.Graphics.Format,System.Int32,System.Int32)
; Implemented: System.Void Android.Views.ISurfaceHolderCallback::SurfaceChanged(Android.Views.ISurfaceHolder,Android.Graphics.Format,System.Int32,System.Int32)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1surfaceChanged(ptr noundef %env, ptr noundef %klass, ptr noundef %0, i32 noundef %1, i32 noundef %2, i32 noundef %3) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_surfaceChanged_0_6_6000348, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %4
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 6, i32 noundef 100664136, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_surfaceChanged_0_6_6000348)
	%cb2 = load ptr, ptr @native_cb_surfaceChanged_0_6_6000348, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %4
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %4]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0, i32 noundef %1, i32 noundef %2, i32 noundef %3)
	ret void
}

; Method: System.Void Android.Views.ISurfaceHolderCallbackInvoker::n_SurfaceCreated_Landroid_view_SurfaceHolder__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Views.ISurfaceHolderCallback::SurfaceCreated(Android.Views.ISurfaceHolder)
; Implemented: System.Void Android.Views.ISurfaceHolderCallback::SurfaceCreated(Android.Views.ISurfaceHolder)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1surfaceCreated(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_surfaceCreated_0_6_6000349, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 6, i32 noundef 100664137, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_surfaceCreated_0_6_6000349)
	%cb2 = load ptr, ptr @native_cb_surfaceCreated_0_6_6000349, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret void
}

; Method: System.Void Android.Views.ISurfaceHolderCallbackInvoker::n_SurfaceDestroyed_Landroid_view_SurfaceHolder__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Views.ISurfaceHolderCallback::SurfaceDestroyed(Android.Views.ISurfaceHolder)
; Implemented: System.Void Android.Views.ISurfaceHolderCallback::SurfaceDestroyed(Android.Views.ISurfaceHolder)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1surfaceDestroyed(ptr noundef %env, ptr noundef %klass, ptr noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_surfaceDestroyed_0_6_600034a, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 6, i32 noundef 100664138, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_surfaceDestroyed_0_6_600034a)
	%cb2 = load ptr, ptr @native_cb_surfaceDestroyed_0_6_600034a, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0)
	ret void
}

; Method: System.SByte Android.Views.View/IOnTouchListenerInvoker::n_OnTouch_Landroid_view_View_Landroid_view_MotionEvent__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Boolean Android.Views.View/IOnTouchListener::OnTouch(Android.Views.View,Android.Views.MotionEvent)
; Implemented: System.Boolean Android.Views.View/IOnTouchListener::OnTouch(Android.Views.View,Android.Views.MotionEvent)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define i1 @Java_crc64493ac3851fab1842_MonoGameAndroidGameView_n_1onTouch(ptr noundef %env, ptr noundef %klass, ptr noundef %0, ptr noundef %1) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onTouch_0_7_60002ce, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %2
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 7, i32 noundef 100664014, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onTouch_0_7_60002ce)
	%cb2 = load ptr, ptr @native_cb_onTouch_0_7_60002ce, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %2
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %2]
	%3 = tail call noundef i1 %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0, ptr noundef %1)
	ret i1 %3
}

; Method: System.Void Android.Views.OrientationEventListener::n_OnOrientationChanged_I_mm_wrapper(System.IntPtr,System.IntPtr,System.Int32)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Views.OrientationEventListener::OnOrientationChanged(System.Int32)
; Implemented: System.Void Microsoft.Xna.Framework.OrientationListener::OnOrientationChanged(System.Int32)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_OrientationListener_n_1onOrientationChanged(ptr noundef %env, ptr noundef %klass, i32 noundef %0) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onOrientationChanged_0_8_60003f1, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %1
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 8, i32 noundef 100664305, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onOrientationChanged_0_8_60003f1)
	%cb2 = load ptr, ptr @native_cb_onOrientationChanged_0_8_60003f1, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %1
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %1]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, i32 noundef %0)
	ret void
}

; Method: System.Void Android.Content.BroadcastReceiver::n_OnReceive_Landroid_content_Context_Landroid_content_Intent__mm_wrapper(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: System.Void Android.Content.BroadcastReceiver::OnReceive(Android.Content.Context,Android.Content.Intent)
; Implemented: System.Void Microsoft.Xna.Framework.ScreenReceiver::OnReceive(Android.Content.Context,Android.Content.Intent)
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_crc64493ac3851fab1842_ScreenReceiver_n_1onReceive(ptr noundef %env, ptr noundef %klass, ptr noundef %0, ptr noundef %1) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_onReceive_0_9_6000624, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %2
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 9, i32 noundef 100664868, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_onReceive_0_9_6000624)
	%cb2 = load ptr, ptr @native_cb_onReceive_0_9_6000624, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %2
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %2]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %0, ptr noundef %1)
	ret void
}

; Method: System.Void Java.Interop.TypeManager/JavaTypeManager::n_Activate_mm(System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr,System.IntPtr)
; Assembly: Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065
; Registered: none
; Implemented: none
;
; Function attributes: "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" uwtable
define void @Java_mono_android_TypeManager_n_1activate(ptr noundef %env, ptr noundef %klass, ptr noundef %jnienv, ptr noundef %jclass, ptr noundef %typename_ptr, ptr noundef %signature_ptr) local_unnamed_addr #3
{
	%cb1 = load ptr, ptr @native_cb_activate_0_10_6000868, align 8, !tbaa !3
	%isNull = icmp eq ptr %cb1, null
	br i1 %isNull, label %loadCallback, label %callbackLoaded

loadCallback: ; preds = %0
	%get_func_ptr = load ptr, ptr @get_function_pointer, align 8, !tbaa !3
	call void %get_func_ptr(i32 noundef 30, i32 noundef 10, i32 noundef 100665448, ptr nonnull noundef align(8) dereferenceable(8) @native_cb_activate_0_10_6000868)
	%cb2 = load ptr, ptr @native_cb_activate_0_10_6000868, align 8, !tbaa !3
	br label %callbackLoaded

callbackLoaded: ; preds = %loadCallback, %0
	%fn = phi ptr
		 [%cb2, %loadCallback],
		 [%cb1, %0]
	tail call void %fn(ptr noundef %env, ptr noundef %klass, ptr noundef %jnienv, ptr noundef %jclass, ptr noundef %typename_ptr, ptr noundef %signature_ptr)
	ret void
}

; Strings
@.mm.0 = private unnamed_addr constant [136 x i8] c"Android.Media.MediaPlayer/IOnCompletionListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.1 = private unnamed_addr constant [101 x i8] c"Java.IO.InputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.2 = private unnamed_addr constant [102 x i8] c"Java.IO.OutputStream, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.3 = private unnamed_addr constant [108 x i8] c"Java.Lang.IRunnableInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.4 = private unnamed_addr constant [102 x i8] c"Android.App.Activity, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.5 = private unnamed_addr constant [100 x i8] c"Android.Views.View, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.6 = private unnamed_addr constant [125 x i8] c"Android.Views.ISurfaceHolderCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.7 = private unnamed_addr constant [124 x i8] c"Android.Views.View/IOnTouchListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.8 = private unnamed_addr constant [120 x i8] c"Android.Views.OrientationEventListener, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.9 = private unnamed_addr constant [115 x i8] c"Android.Content.BroadcastReceiver, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.10 = private unnamed_addr constant [122 x i8] c"Java.Interop.TypeManager/JavaTypeManager, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065\00", align 1
@.mm.11 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [76 x i8] c"n_OnCompletion_Landroid_media_MediaPlayer__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.1_name = private unnamed_addr constant [34 x i8] c"n_Close_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.2_name = private unnamed_addr constant [33 x i8] c"n_Read_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.3_name = private unnamed_addr constant [47 x i8] c"n_Read_arrayB_mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.4_name = private unnamed_addr constant [61 x i8] c"n_Read_arrayBII_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32)\00", align 1
@.MarshalMethodName.5_name = private unnamed_addr constant [34 x i8] c"n_Flush_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.6_name = private unnamed_addr constant [48 x i8] c"n_Write_arrayB_mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.7_name = private unnamed_addr constant [62 x i8] c"n_Write_arrayBII_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32)\00", align 1
@.MarshalMethodName.8_name = private unnamed_addr constant [42 x i8] c"n_Write_I_mm_wrapper(IntPtr,IntPtr,Int32)\00", align 1
@.MarshalMethodName.9_name = private unnamed_addr constant [32 x i8] c"n_Run_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.10_name = private unnamed_addr constant [64 x i8] c"n_OnCreate_Landroid_os_Bundle__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.11_name = private unnamed_addr constant [94 x i8] c"n_OnConfigurationChanged_Landroid_content_res_Configuration__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.12_name = private unnamed_addr constant [36 x i8] c"n_OnPause_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.13_name = private unnamed_addr constant [37 x i8] c"n_OnResume_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.14_name = private unnamed_addr constant [38 x i8] c"n_OnDestroy_mm_wrapper(IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.15_name = private unnamed_addr constant [76 x i8] c"n_OnKeyDown_ILandroid_view_KeyEvent__mm_wrapper(IntPtr,IntPtr,Int32,IntPtr)\00", align 1
@.MarshalMethodName.16_name = private unnamed_addr constant [74 x i8] c"n_OnKeyUp_ILandroid_view_KeyEvent__mm_wrapper(IntPtr,IntPtr,Int32,IntPtr)\00", align 1
@.MarshalMethodName.17_name = private unnamed_addr constant [83 x i8] c"n_OnGenericMotionEvent_Landroid_view_MotionEvent__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.18_name = private unnamed_addr constant [100 x i8] c"n_SurfaceChanged_Landroid_view_SurfaceHolder_III_mm_wrapper(IntPtr,IntPtr,IntPtr,Int32,Int32,Int32)\00", align 1
@.MarshalMethodName.19_name = private unnamed_addr constant [79 x i8] c"n_SurfaceCreated_Landroid_view_SurfaceHolder__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.20_name = private unnamed_addr constant [81 x i8] c"n_SurfaceDestroyed_Landroid_view_SurfaceHolder__mm_wrapper(IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.21_name = private unnamed_addr constant [96 x i8] c"n_OnTouch_Landroid_view_View_Landroid_view_MotionEvent__mm_wrapper(IntPtr,IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.22_name = private unnamed_addr constant [57 x i8] c"n_OnOrientationChanged_I_mm_wrapper(IntPtr,IntPtr,Int32)\00", align 1
@.MarshalMethodName.23_name = private unnamed_addr constant [102 x i8] c"n_OnReceive_Landroid_content_Context_Landroid_content_Intent__mm_wrapper(IntPtr,IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.24_name = private unnamed_addr constant [57 x i8] c"n_Activate_mm(IntPtr,IntPtr,IntPtr,IntPtr,IntPtr,IntPtr)\00", align 1
@.MarshalMethodName.25_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nofree norecurse nosync nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { "no-trapping-math"="true" noreturn nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }
attributes #3 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable }

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ db080b91dbe7947a69005a203ea98d9bb207ad04"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}

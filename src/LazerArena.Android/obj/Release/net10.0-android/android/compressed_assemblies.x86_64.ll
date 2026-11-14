; ModuleID = 'compressed_assemblies.x86_64.ll'
source_filename = "compressed_assemblies.x86_64.ll"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-linux-android21"

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	i32 ; uint32_t buffer_offset
}

@compressed_assembly_count = dso_local local_unnamed_addr constant i32 31, align 4

@compressed_assembly_descriptors = dso_local local_unnamed_addr global [31 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 2560, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 0; uint32_t buffer_offset
	}, ; 0: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 585728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 2560; uint32_t buffer_offset
	}, ; 1: MonoGame.Framework
	%struct.CompressedAssemblyDescriptor {
		i32 52224, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 588288; uint32_t buffer_offset
	}, ; 2: LazerArena.Core
	%struct.CompressedAssemblyDescriptor {
		i32 477184, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 640512; uint32_t buffer_offset
	}, ; 3: LazerArena.Presentation
	%struct.CompressedAssemblyDescriptor {
		i32 38912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1117696; uint32_t buffer_offset
	}, ; 4: LazerArena.Android
	%struct.CompressedAssemblyDescriptor {
		i32 23040, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1156608; uint32_t buffer_offset
	}, ; 5: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 30208, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1179648; uint32_t buffer_offset
	}, ; 6: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1209856; uint32_t buffer_offset
	}, ; 7: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1215488; uint32_t buffer_offset
	}, ; 8: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1220608; uint32_t buffer_offset
	}, ; 9: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 6656, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1232896; uint32_t buffer_offset
	}, ; 10: System.Drawing.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1239552; uint32_t buffer_offset
	}, ; 11: System.Drawing
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1244672; uint32_t buffer_offset
	}, ; 12: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1262080; uint32_t buffer_offset
	}, ; 13: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 39424, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1268224; uint32_t buffer_offset
	}, ; 14: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 14336, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1307648; uint32_t buffer_offset
	}, ; 15: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1321984; uint32_t buffer_offset
	}, ; 16: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1327104; uint32_t buffer_offset
	}, ; 17: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 67072, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1333248; uint32_t buffer_offset
	}, ; 18: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1400320; uint32_t buffer_offset
	}, ; 19: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1409536; uint32_t buffer_offset
	}, ; 20: System.Runtime.Serialization.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 8192, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1415168; uint32_t buffer_offset
	}, ; 21: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 8704, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1423360; uint32_t buffer_offset
	}, ; 22: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 29696, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1432064; uint32_t buffer_offset
	}, ; 23: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 352768, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1461760; uint32_t buffer_offset
	}, ; 24: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 150528, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1814528; uint32_t buffer_offset
	}, ; 25: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1965056; uint32_t buffer_offset
	}, ; 26: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 1659904, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 1977344; uint32_t buffer_offset
	}, ; 27: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 162304, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3637248; uint32_t buffer_offset
	}, ; 28: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 22328, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3799552; uint32_t buffer_offset
	}, ; 29: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 423936, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		i32 3821880; uint32_t buffer_offset
	} ; 30: Mono.Android
], align 16

@uncompressed_assemblies_data_size = dso_local local_unnamed_addr constant i32 4245816, align 4

@uncompressed_assemblies_data_buffer = dso_local local_unnamed_addr global [4245816 x i8] zeroinitializer, align 16

; Metadata
!llvm.module.flags = !{!0, !1}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/10.0.1xx @ db080b91dbe7947a69005a203ea98d9bb207ad04"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}

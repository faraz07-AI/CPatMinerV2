package main;

import java.io.File;

import graphics.DotGraph;

import javax.swing.plaf.synth.SynthDesktopIconUI;

public class GenerateImageFromDotFile {

	public static void main(String[] args) {
		File dir = new File("/Users/farazgurramkonda/IdeaProjects/CPatMinerV2/CPatMinerV2/AtomicASTChangeMining/src/main/output/patterns/outputs-hybrid/1");
		for (File file : dir.listFiles()) {
			System.out.println(file);
//			System.out.println(level);
//			for (File size : level.listFiles()) {
//				System.out.println(size);
//				if (size.isDirectory()) {
//					for (File p : size.listFiles()) {
//						for (File file : p.listFiles()) {
							if (file.getName().endsWith(".dot")) {
								String name = file.getName().substring(0, file.getName().length() - ".dot".length());
								System.out.println(name);
								File image = new File(file, name + ".png");
								if (!image.exists()) {
									System.out.println();
									DotGraph.toGraphics(file.getAbsolutePath() + "/" + name, "png");
								}
							}
						}
					}
				}
//			}
//		}
//	}

//}

package main;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Random;

import utils.FileIO;

public class SamplePatterns {

	public static void main(String[] args) throws IOException {
		ArrayList<File> patternDirs = new ArrayList<>();
		final HashMap<File, Integer> frequencies = new HashMap<>(), sizes = new HashMap<>();
		final HashMap<File, Double> scores = new HashMap<>();
		File dir = new File("/Users/farazgurramkonda/IdeaProjects/CPatMinerV2/CPatMinerV2/AtomicASTChangeMining/src/main/output/patterns/outputs-hybrid/1");
		for (File dirSize : dir.listFiles()) {
			if (!dirSize.isDirectory())
				continue;
			int size = Integer.parseInt(dirSize.getName());
			for (File dirPattern : dirSize.listFiles()) {
				if (isGood(dirPattern)) {
					patternDirs.add(dirPattern);
					sizes.put(dirPattern, size);
					System.out.println(dirPattern);
					int f = readFrequency(dirPattern);
					frequencies.put(dirPattern, f);
					scores.put(dirPattern, Math.sqrt(size) * f);
				}
			}
		}
		Random r = new Random();
		File[] samples = new File[patternDirs.size()];
		System.out.println("length:"+samples.length);
		for (int i = 0; i < samples.length; i++) {
			int index = r.nextInt(patternDirs.size());
			System.out.println("pat_size:"+patternDirs.size());
			System.out.println("index:"+index);
			samples[i] = patternDirs.get(index);
			//patternDirs.remove(index);
			System.out.println("p:"+patternDirs.get(i));
			System.out.println("sam:"+samples[i]);
			samples[i] = patternDirs.get(i);
		}
		
		dir = new File("/Users/farazgurramkonda/IdeaProjects/CPatMinerV2/CPatMinerV2/AtomicASTChangeMining/pattern-samp");
		if (!dir.exists())
			dir.mkdir();
		for (int i = 0; i < samples.length; i++) {
			File p = samples[i];
			File pd = new File(dir, p.getParentFile().getParentFile().getParentFile().getName() + "/" + (i+1) + "/" + p.getParentFile().getName() + "/" + p.getName());
			pd.mkdirs();
			copyDirectory(p, pd);
		}
	}

	private static int readFrequency(File dirPattern) {
		String content = FileIO.readStringFromFile(dirPattern.getAbsolutePath() + "/details.html");
		if(content==null){
			return 0;
		}
		String p = "<a href=";
		System.out.println(p);
		int c = 0, i = 0;
		while (true) {
			i = content.indexOf(p, i);
			if (i == -1)
				break;
			c++;
			i += p.length();
		}
		return c;
	}

	private static void copyDirectory(File p, File pd) {
		for (File file : p.listFiles()) {
			try {
				Files.copy(Paths.get(file.getAbsolutePath()), Paths.get(pd.getAbsolutePath() + "/" + file.getName()), StandardCopyOption.REPLACE_EXISTING);
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

	private static boolean isGood(File dirPattern) {
		return true;
	}

}

//package exas;
//
//import java.util.ArrayList;
//import java.util.HashMap;
//
//import codemining.Feature;
//import groum.GROUMNode;
//
//public abstract class ExasFeature implements Feature {
//	protected static int numOfFeatures = 0, numOfBranches = 0;
//	public static int MAX_LENGTH = 4;
//
//	protected int hash;
//
//	protected HashMap<ExasSingleFeature, ExasSequentialFeature> next = new HashMap<ExasSingleFeature, ExasSequentialFeature>();
//
//	abstract public int getFeatureLength();
//
//	protected int id, frequency = 0;
//
//	protected ExasFeature(ArrayList<GROUMNode> nodes) {
//		this.id = ++numOfFeatures;
//	}
//
//	public static int getNumOfFeatures() {
//		return numOfFeatures;
//	}
//
//	@Override
//	public int getId() {
//		return id;
//	}
//
//	public int getFrequency() {
//		return frequency;
//	}
//
//	public void setFrequency(int frequency) {
//		this.frequency = frequency;
//	}
//
//	public static ExasFeature getFeature(ArrayList<ExasSingleFeature> sequence)
//	{
//		if (sequence.size() == 1)
//			return sequence.get(0);
//		ExasFeature feature = sequence.get(0);
//		int i = 1;
//		while (i < sequence.size())
//		{
//			ExasSingleFeature s = sequence.get(i);
//			if (feature.next.containsKey(s))
//			{
//				feature = feature.next.get(s);
//			}
//			else
//			{
//				feature = new ExasSequentialFeature(sequence.subList(0, i+1), feature, s);
//			}
//			i++;
//		}
//		return feature;
//	}
//
////	public static int getFeature(String label)
////	{
////		ExasSingleFeature f = ExasSingleFeature.features.get(label);
////		if (f == null)
////			f = new ExasSingleFeature(label);
////		return f;
////	}
////
//	private HashMap<String, Integer> nodeFeatures = new HashMap<>();
//
//	private int getEdgeFeature(String label) {
//		Integer id = edgeFeatures.get(label);
//		if (id != null)
//			return id;
//		return 0;
//	}
//
//	public int getFeature(ArrayList<String> labels) {
//		int f = 0, s;
//		for (int i = 0; i < labels.size(); i++) {
//			if (i % 2 == 0) {
//				s = getNodeFeature(labels.get(i));
//			}
//			else {
//				s = getEdgeFeature(labels.get(i));
//				s = s << 5;
//				f = f << 8;
//			}
//			f += s;
//		}
//		return f;
//	}
//
//	@Override
//	abstract public int hashCode();
//
//	@Override
//	abstract public boolean equals(Object obj);
//
//	@Override
//	abstract public String toString();
//
//	public int compareTo(ExasFeature other) {
//		if (getFeatureLength() < other.getFeatureLength())
//			return -1;
//		if (getFeatureLength() > other.getFeatureLength())
//			return 1;
//		if (this instanceof ExasSingleFeature)
//		{
//			return ((ExasSingleFeature)this).getLabel().compareTo(((ExasSingleFeature)other).getLabel());
//		}
//		for (int i = 0; i < getFeatureLength(); i++)
//		{
//			int c = ((ExasSequentialFeature)this).getSequence().get(i).getLabel().compareTo(((ExasSequentialFeature)other).getSequence().get(i).getLabel());
//			if (c != 0)
//				return c;
//		}
//		return 0;
//	}
//}


package exas;

import groum.GROUMNode;

import java.util.ArrayList;
import java.util.HashMap;

public class ExasFeature {
	public static final int MAX_LENGTH = 4 * 2 - 1;
	private static HashMap<String, Integer> edgeFeatures = new HashMap<>();
	static {
		edgeFeatures.put("_qual_", 0);
		edgeFeatures.put("_cond_", 1);
		edgeFeatures.put("_control_", 2);
		edgeFeatures.put("_def_",  3);
		edgeFeatures.put("_map_",  4);
		edgeFeatures.put("_para_", 5);
		edgeFeatures.put("_recv_", 6);
		edgeFeatures.put("_ref_", 7);
	}

	public int value;

	private HashMap<String, Integer> nodeFeatures = new HashMap<>();

	public ExasFeature(ArrayList<GROUMNode> nodes) {
		for (int i = 0; i < nodes.size(); i++)
			nodeFeatures.put(nodes.get(i).getLabel(), i + 1);
	}

	private int getNodeFeature(String label) {
		if(nodeFeatures.get(label)==null){
			return 0;
		};
		return nodeFeatures.get(label);
	}

	private int getEdgeFeature(String label) {
		Integer id = edgeFeatures.get(label);
		if (id != null)
			return id;
		return 0;
	}

	public int getFeature(ArrayList<String> labels) {
		int f = 0, s;
		for (int i = 0; i < labels.size(); i++) {
			if (i % 2 == 0) {
				s = getNodeFeature(labels.get(i));
			}
			else {
				s = getEdgeFeature(labels.get(i));
				s = s << 5;
				f = f << 8;
			}
			f += s;
		}
		return f;
	}

	public int getFeature(String label) {
		return getNodeFeature(label);
	}
		public int compareTo(ExasFeature other) {
		if (getFeatureLength() < other.getFeatureLength())
			return -1;
		if (getFeatureLength() > other.getFeatureLength())
			return 1;
		if (this instanceof ExasSingleFeature)
		{
			return ((ExasSingleFeature)this).getLabel().compareTo(((ExasSingleFeature)other).getLabel());
		}
		return 0;
	}

	private int getFeatureLength() {
		return 0;
	}

}

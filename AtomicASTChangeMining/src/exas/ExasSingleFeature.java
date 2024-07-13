package exas;

import java.util.HashMap;

public class ExasSingleFeature extends ExasFeature {
	protected HashMap<String, ExasSingleFeature> nodes;

	public static HashMap<String, ExasSingleFeature> features = new HashMap<String, ExasSingleFeature>();
	private String label;

	public ExasSingleFeature(String label) {
        super(null);
        //super(nodes);
		this.label = label;
		features.put(label, this);
	}

	public ExasSingleFeature(int labelId) {
		super(null);
		this.label = String.valueOf(labelId);
		features.put(label, this);
	}

	public String getLabel() {
		return label;
	}

	public void setLabel(String label) {
		this.label = label;
	}


	public int getFeatureLength() {
		return 1;
	}

	@Override
	public String toString() {
		return this.label;
	}

	@Override
	public int hashCode() {
		int hash = 0;
		if (hash == 0 && label.length() > 0)
			hash = label.hashCode();
		return hash;
	}

	@Override
	public boolean equals(Object obj) {
		if (obj == null) return false;
		if (obj == this) return true;
		if (obj instanceof ExasSingleFeature) {
			return this.label.equals(((ExasSingleFeature) obj).label);
		}
		return false;
	}
}

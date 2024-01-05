package transformation;

import com.github.gumtreediff.tree.Tree;
import com.github.gumtreediff.tree.TreeContext;
import com.github.gumtreediff.client.Run;
import com.github.gumtreediff.gen.srcml.SrcmlCsTreeGenerator;
import org.eclipse.jdt.core.dom.*;

public class Transformation {

    private static void iterate_children(Tree root, SrcMLTreeVisitor visitor) {
        if (root == null) {
            return;
        } else if (root instanceof UnitNode) {
            CompilationUnit m = visitor.visit((UnitNode) root);
            System.out.println(m.toString());
        } else {
            for (Tree child : root.getChildren()) {
                iterate_children(child, visitor);
            }
        }
    }

    public static void transform() {

        Run.initGenerators(); // registers the available parsers
        String thisfile = "test.cs";
        String yaml_file = "test.yaml";
        String java_file = "test.java";
        try {
            SrcmlCsTreeGenerator l = new SrcmlCsTreeGenerator();
            TreeContext tc = l.generateFrom().file(thisfile);
            Tree tree_csharp = tc.getRoot();
            //Tree tree_yaml = new YamlTreeGenerator().generateFrom().file(yaml_file).getRoot();

            System.out.println(tree_csharp.toTreeString());
            Tree transformedTree = TransformationUtils.transformTree(tree_csharp);
            transformedTree.toString();

            SrcMLTreeVisitor visitor = new SrcMLTreeVisitor();
            iterate_children(transformedTree, visitor);

        } catch (Exception e) {
            throw new RuntimeException(e);
        }


    }
}

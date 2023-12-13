package transformation;

import com.github.gumtreediff.tree.Tree;
import com.github.gumtreediff.tree.TreeContext;
import com.github.gumtreediff.client.Run;
import com.github.gumtreediff.gen.srcml.SrcmlCsTreeGenerator;
import com.github.gumtreediff.gen.jdt.JdtTreeGenerator;
import org.eclipse.jdt.core.dom.AST;

import transformation.SrcMLToJavaASTTransformation.*;

import java.util.ArrayList;

public class Transformation {

    private static void transformNode(Tree node){
        System.out.println("********************");
        //System.out.println("position  " + node.getPos());
        //System.out.println("label  " + node.getLabel());
        //System.out.println("type  " + node.getType());
    }

    private static void iterate_children(AST asn, Tree root){
        if (root == null) {
            return;
        }
        //transformNode(root);
        Object arr = SrcMLToJavaASTTransformation.getNodeMapping(asn, root);

        for( Tree child: root.getChildren()){
            iterate_children(asn, child);
        }
    }

    public static void transform(){

        Run.initGenerators(); // registers the available parsers
        String thisfile = "test.cs";
        String yaml_file = "test.yaml";
        String java_file = "test.java";
        try {
            SrcmlCsTreeGenerator l = new SrcmlCsTreeGenerator();
            TreeContext tc = l.generateFrom().file(thisfile);
            Tree tree_csharp = tc.getRoot();

            //Tree tree_yaml = new YamlTreeGenerator().generateFrom().file(yaml_file).getRoot();
            //Tree tree_java = new JdtTreeGenerator().generateFrom().file(java_file).getRoot();

            System.out.println(tree_csharp.toTreeString());
            AST asn = new AST();

            iterate_children(asn, tree_csharp);

        } catch (Exception e) {
            throw new RuntimeException(e);
        }







    }
}

package transformation;

import com.github.gumtreediff.tree.Tree;
import com.github.gumtreediff.tree.TreeContext;
import com.github.gumtreediff.client.Run;
import com.github.gumtreediff.gen.srcml.SrcmlCsTreeGenerator;
import com.github.gumtreediff.gen.jdt.JdtTreeGenerator;
import org.eclipse.jdt.core.dom.*;
import org.eclipse.jdt.core.dom.AST;
import transformation.SrcMLToJavaASTTransformation.*;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.util.ArrayList;

public class Transformation {

    private static void iterate_children(AST asn, Tree root) {
        if (root == null) {
            return;
        }
        //transformNode(root);
        Object arr = SrcMLToJavaASTTransformation.getNodeMapping(asn, root);

        for (Tree child : root.getChildren()) {
            iterate_children(asn, child);
        }
    }

    private static void iterate_children_test(AST asn, Tree root, SrcMLTreeVisitor visitor) {
        if (root == null) {
            return;
        } else if (root instanceof CallNode) {
            MethodInvocation m = visitor.visit((CallNode) root);
            System.out.println(m.getName());
        } else {
            for (Tree child : root.getChildren()) {
                iterate_children_test(asn, child, visitor);
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
            AST asn = new AST();
            Tree transformedTree = TransformationUtils.transformTree(tree_csharp);
            transformedTree.toString();

            SrcMLTreeVisitor visitor = new SrcMLTreeVisitor();
            //iterate_children_test(asn, transformedTree, visitor);

            ASTParser parser = ASTParser.newParser(AST.JLS8);
            parser.setKind(ASTParser.K_COMPILATION_UNIT);
            File javaFile = new File(java_file);
            BufferedReader in = new BufferedReader(new FileReader(javaFile));
            final StringBuffer buffer = new StringBuffer();
            String line = null;
            while (null != (line = in.readLine())) {
                buffer.append(line).append("\n");
            }
            parser.setSource(buffer.toString().toCharArray());
            CompilationUnit compilationUnit = (CompilationUnit) parser.createAST(null);
            System.out.println(compilationUnit.toString());


        } catch (Exception e) {
            throw new RuntimeException(e);
        }


    }
}

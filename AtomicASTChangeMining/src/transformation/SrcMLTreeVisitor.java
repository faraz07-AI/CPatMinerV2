package transformation;

import com.github.gumtreediff.tree.Tree;
import utils.Pair;

import java.util.ArrayDeque;
import java.util.Iterator;

public class SrcMLTreeVisitor {

    /*public void visitTree(Tree root) {
        Deque<Pair<Tree, Iterator<Tree>>> stack = new ArrayDeque<>();
        stack.push(new Pair<>(root, root.getChildren().iterator()));
        //visitor.startTree(root);
        typeList.add(root.getType().name);
        labelList.add(root.getLabel());
        while (!stack.isEmpty()) {
            Pair<ITree, Iterator<ITree>> it = stack.peek();

            if (!it.second.hasNext()) {
                //visitor.endTree(it.first);
                stack.pop();
            } else {
                ITree child = it.second.next();
                stack.push(new Pair<>(child, child.getChildren().iterator()));
                // visitor.startTree(child);
                typeList.add(child.getType().name);
                labelList.add(child.getLabel());
            }
        }
    }*/

}

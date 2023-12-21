package transformation;

import com.github.gumtreediff.tree.Tree;
import org.eclipse.jdt.core.dom.AST;
import org.eclipse.jdt.core.dom.StringLiteral;
import org.eclipse.jdt.core.dom.Expression;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class TransformationUtils {

    static String capitalizeFirstLetter(String input) {
        if (input == null || input.isEmpty())
            return input;
        return input.substring(0, 1).toUpperCase() + input.substring(1);
    }

    public static boolean isBoolean(String input) {
        return Objects.equals(input, "true") || Objects.equals(input, "false");
    }

    public static boolean isInteger(String input) {
        try {
            Integer.parseInt(input);
            return true;
        } catch (NumberFormatException e) {
            return false;
        }
    }

    static Expression type_literal(AST asn, String input) {
        if (isBoolean(input)) {
            return asn.newBooleanLiteral(Boolean.parseBoolean(input));
        } else if (isInteger(input)) {
            return asn.newNumberLiteral(input);
        } else {
            StringLiteral stringLiteral = asn.newStringLiteral();
            stringLiteral.setLiteralValue(input);
            return stringLiteral;
        }
    }


    public static Tree transformTree(Tree inputTree) {
        String nodeType = inputTree.getType().toString();
        List<Tree> children = inputTree.getChildren();
        children = transformChildren(children);
        Tree new_tree;
        switch (nodeType) {
            case SrcMLNodeType.ARGUMENT:
                 new_tree = new ArgumentNode(inputTree);
            case SrcMLNodeType.NAME:
                new_tree = new NameNode(inputTree);
            case SrcMLNodeType.COMMENT:
                new_tree = new CommentNode(inputTree);
            case SrcMLNodeType.NAMESPACE:
                new_tree = new NamespaceNode(inputTree);
            case SrcMLNodeType.ARGUMENT_LIST:
                new_tree = new ArgumentListNode(inputTree);
            case SrcMLNodeType.CALL:
                new_tree = new CallNode(inputTree);
            case SrcMLNodeType.EXPR:
                new_tree = new ExprNode(inputTree);
            case SrcMLNodeType.EXPR_STMT:
                new_tree = new ExprStmtNode(inputTree);
            case SrcMLNodeType.LITERAL:
                new_tree = new LiteralNode(inputTree);
            case SrcMLNodeType.UNIT:
                new_tree = new UnitNode(inputTree);
            case SrcMLNodeType.USING:
                new_tree = new UsingNode(inputTree);
            case SrcMLNodeType.CLASS:
                new_tree = new ClassNode(inputTree);


            case SrcMLNodeType.BLOCK:
                new_tree = new BlockNode(inputTree);
            case SrcMLNodeType.FUNCTION:
                new_tree = new FunctionNode(inputTree);
            case SrcMLNodeType.TYPE:
                new_tree = new TypeNode(inputTree);
            case SrcMLNodeType.SPECIFIER:
                new_tree = new ParameterListNode(inputTree);
            case SrcMLNodeType.PARAMETER_LIST:
                new_tree = new ParameterNode(inputTree);
            case SrcMLNodeType.PARAMETER:
                new_tree = new ParameterNode(inputTree);
            case SrcMLNodeType.BLOCK_CONTENT:
                new_tree = new BlockContentNode(inputTree);
            case SrcMLNodeType.OPERATOR:
                new_tree = new OperatorNode(inputTree);
            case SrcMLNodeType.ESCAPE:
                new_tree = new EscapeNode(inputTree);
            case SrcMLNodeType.BREAK:
                new_tree = new BreakNode(inputTree);
            case SrcMLNodeType.CASE:
                new_tree = new CaseNode(inputTree);
            case SrcMLNodeType.CONTINUE:
                new_tree = new ContinueNode(inputTree);

            case SrcMLNodeType.DEFAULT:
                new_tree = new DefaultNode(inputTree);
            case SrcMLNodeType.DO:
                new_tree = new DoNode(inputTree);
            case SrcMLNodeType.EMPTY_STMT:
                new_tree = new EmptyStmtNode(inputTree);
            case SrcMLNodeType.FIXED:
                new_tree = new FixedNode(inputTree);
            case SrcMLNodeType.FOR:
                new_tree = new ForNode(inputTree);
            case SrcMLNodeType.FOREACH:
                new_tree = new ForeachNode(inputTree);
            case SrcMLNodeType.GOTO:
                new_tree = new GotoNode(inputTree);
            case SrcMLNodeType.IF_STMT:
                new_tree = new IfStmtNode(inputTree);
            case SrcMLNodeType.LABEL:
                new_tree = new LabelNode(inputTree);
            case SrcMLNodeType.LOCK:
                new_tree = new LockNode(inputTree);
            case SrcMLNodeType.RETURN:
                new_tree = new ReturnNode(inputTree);
            case SrcMLNodeType.SWITCH:
                new_tree = new SwitchNode(inputTree);

            case SrcMLNodeType.UNSAFE:
                new_tree = new UnsafeNode(inputTree);
            case SrcMLNodeType.USING_STMT:
                new_tree = new UsingStmtNode(inputTree);
            case SrcMLNodeType.WHILE:
                new_tree = new WhileNode(inputTree);
            case SrcMLNodeType.CONDITION:
                new_tree = new ConditionNode(inputTree);
            case SrcMLNodeType.CONTROL:
                new_tree = new ControlNode(inputTree);
            case SrcMLNodeType.ELSE:
                new_tree = new ElseNode(inputTree);
            case SrcMLNodeType.INCR:
                new_tree = new IncrNode(inputTree);
            case SrcMLNodeType.THEN:
                new_tree = new ThenNode(inputTree);
            case SrcMLNodeType.INIT:
                new_tree = new InitNode(inputTree);
            case SrcMLNodeType.DELEGATE:
                new_tree = new DelegateNode(inputTree);
            case SrcMLNodeType.FUNCTION_DECL:
                new_tree = new FunctionDeclNode(inputTree);
            case SrcMLNodeType.LAMBDA:
                new_tree = new LambdaNode(inputTree);

            case SrcMLNodeType.MODIFIER:
                new_tree = new ModifierNode(inputTree);
            case SrcMLNodeType.DECL:
                new_tree = new DeclNode(inputTree);
            case SrcMLNodeType.DECL_STMT:
                new_tree = new DeclStmtNode(inputTree);
            case SrcMLNodeType.CONSTRUCTOR:
                new_tree = new ConstructorNode(inputTree);
            case SrcMLNodeType.DESTRCUCTOR:
                new_tree = new DestrctorNode(inputTree);
            case SrcMLNodeType.ENUM:
                new_tree = new EnumNode(inputTree);
            case SrcMLNodeType.EVENT:
                new_tree = new EventNode(inputTree);
            case SrcMLNodeType.SUPER_LIST:
                new_tree = new SuperListNode(inputTree);
            case SrcMLNodeType.INTERFACE:
                new_tree = new InterfaceNode(inputTree);
            case SrcMLNodeType.PROPERTY:
                new_tree = new PropertyNode(inputTree);
            case SrcMLNodeType.STRUCT:
                new_tree = new StructNode(inputTree);
            case SrcMLNodeType.TERNARY:
                new_tree = new TernaryNode(inputTree);

            case SrcMLNodeType.ATTRIBUTE:
                new_tree = new AttributeNode(inputTree);
            case SrcMLNodeType.CHECKED:
                new_tree = new CheckedNode(inputTree);
            case SrcMLNodeType.TYPEOF:
                new_tree = new TypeOfNode(inputTree);
            case SrcMLNodeType.SIZEOF:
                new_tree = new SizeOfNode(inputTree);
            case SrcMLNodeType.UNCHECKED:
                new_tree = new UncheckedNode(inputTree);
            case SrcMLNodeType.CONSTRAINT:
                new_tree = new ConstraintNode(inputTree);
            case SrcMLNodeType.CATCH:
                new_tree = new CatchNode(inputTree);
            case SrcMLNodeType.FINALLY:
                return new FinallyNode(inputTree);

            case SrcMLNodeType.THROW:
                new_tree = new ThrowNode(inputTree);
            case SrcMLNodeType.TRY:
                new_tree = new TryNode(inputTree);
            case SrcMLNodeType.BY:
                new_tree = new ByNode(inputTree);
            case SrcMLNodeType.EQUALS:
                new_tree = new EqualsNode(inputTree);
            case SrcMLNodeType.FROM:
                new_tree = new FromNode(inputTree);
            case SrcMLNodeType.GROUP:
                new_tree = new GroupNode(inputTree);
            case SrcMLNodeType.IN:
                new_tree = new InNode(inputTree);
            case SrcMLNodeType.INTO:
                new_tree = new IntoNode(inputTree);
            case SrcMLNodeType.JOIN:
                new_tree = new JoinNode(inputTree);
            case SrcMLNodeType.LET:
                new_tree = new LetNode(inputTree);
            case SrcMLNodeType.LINQ:
                new_tree = new LinqNode(inputTree);
            case SrcMLNodeType.ON:
                new_tree = new OnNode(inputTree);
            case SrcMLNodeType.ORDERBY:
                new_tree = new OrderByNode(inputTree);
            case SrcMLNodeType.SELECT:
                new_tree = new SelectNode(inputTree);
            case SrcMLNodeType.WHERE:
                new_tree = new WhereNode(inputTree);
            case SrcMLNodeType.IF:
                new_tree = new IfNode(inputTree);
            default:
                new_tree = new SrcMLNodeType(inputTree);
        }
        new_tree.setChildren(children);
        return new_tree;
    }
    public static List<Tree> transformChildren(List<Tree> children) {
        List<Tree> transformedChildren = new ArrayList<>();
        for (Tree child : children) {
            transformedChildren.add(TransformationUtils.transformTree(child));
        }
        return transformedChildren;
    }

}

package transformation;

import com.github.gumtreediff.tree.Tree;
import org.eclipse.jdt.core.dom.*;

import java.util.ArrayList;
import java.util.List;

public class SrcMLTreeVisitor {

    public List<ASTNode> transformed_nodes = new ArrayList<ASTNode>();
    AST asn = new AST();

    void visit(CommentNode node) {
        // Do nothing since comments are not needed
    }
    void visit(UsingNode node) {
        // No need to map it, but we can remove its children though
        node.setChildren(new ArrayList<>());
    }
    Expression visit(LiteralNode node) {
        String init_literal = node.getLabel();
        return TransformationUtils.type_literal(asn, init_literal);
    }
    SimpleName visit(NameNode node) {
        return asn.newSimpleName(node.getLabel());
    }
    MethodInvocation visit(CallNode node) {
        MethodInvocation methodInvocation = asn.newMethodInvocation();
        List<Tree> children = node.getChildren();
        Tree nameNode = children.get(0);
        if (nameNode.getChildren().size()==1){ // if we only have one method call like do(...)
            Tree child = nameNode.getChildren().get(0);
            if (child instanceof NameNode){
                methodInvocation.setName(this.visit((NameNode)child));
            }
        } else { // we have something like obj.do()
            List<Tree> method_calls = nameNode.getChildren();
            if (method_calls.get(0) instanceof NameNode)
                methodInvocation.setExpression(this.visit((NameNode)method_calls.get(0)));
            if (method_calls.get(2) instanceof NameNode)
                methodInvocation.setName(this.visit((NameNode)method_calls.get(2)));
        }
        Tree argNode = children.get(1);
        if (argNode instanceof ArgumentListNode){
            List<Tree> args = argNode.getChildren();
            for(Tree arg: args){
                if (arg instanceof ArgumentNode){ // case of (a,b), need to check: (a,a+b) or (a,new class(xy,xyd))
                    arg = arg.getChildren().get(0);
                    if(arg instanceof ExprNode){
                        methodInvocation.arguments().add(this.visit((ExprNode) arg));
                    }
                }
            }
        }
        return methodInvocation;
    }
    Expression visit(ExprNode node) {
        List<Tree> children = node.getChildren();
        if (children.size()==1 && children.get(0) instanceof LiteralNode){
            return this.visit((LiteralNode) children.get(0));
        }
        else if(children.size()== 3){ // expression with one operator, must update with more
            InfixExpression infixExpression = asn.newInfixExpression();

            infixExpression.setOperator(InfixExpression.Operator.PLUS);

            if (children.get(0) instanceof LiteralNode)
                infixExpression.setLeftOperand(this.visit((LiteralNode) children.get(0)));
            if (children.get(2) instanceof LiteralNode)
                infixExpression.setRightOperand(this.visit((LiteralNode) children.get(2)));
        }
        return null;
    }
    void visit(UnitNode node) {

    }

    void visit(OperatorNode node) {
    }

    void visit(OnNode node) {
    }

    void visit(IfNode node) {

    }

    void visit(NamespaceNode node) {
    }

    void visit(ArgumentListNode node) {
    }

    void visit(ArgumentNode node) {
    }

    void visit(ExprStmtNode node) {
    }
    void visit(ClassNode node) {
    }

    void visit(BlockNode node) {
    }

    void visit(FunctionNode node) {
    }

    void visit(TypeNode node) {
    }

    void visit(SpecifierNode node) {
    }

    void visit(ParameterListNode node) {
    }

    void visit(ParameterNode node) {
    }

    void visit(BlockContentNode node) {
    }

    void visit(EscapeNode node) {
    }

    void visit(BreakNode node) {
    }

    void visit(CaseNode node) {
    }

    void visit(ContinueNode node) {
    }

    void visit(DefaultNode node) {
    }

    void visit(DoNode node) {
    }

    void visit(EmptyStmtNode node) {
    }

    void visit(FixedNode node) {
    }

    void visit(ForNode node) {
    }

    void visit(ForeachNode node) {
    }

    void visit(GotoNode node) {
    }

    void visit(IfStmtNode node) {
    }

    void visit(LabelNode node) {
    }

    void visit(LockNode node) {
    }

    void visit(ReturnNode node) {
    }

    void visit(SwitchNode node) {
    }

    void visit(UnsafeNode node) {
    }

    void visit(UsingStmtNode node) {
    }

    void visit(WhileNode node) {
    }

    void visit(ConditionNode node) {
    }

    void visit(ControlNode node) {
    }

    void visit(ElseNode node) {
    }

    void visit(IncrNode node) {
    }

    void visit(ThenNode node) {
    }

    void visit(InitNode node) {
    }

    void visit(DelegateNode node) {
    }

    void visit(FunctionDeclNode node) {
    }

    void visit(LambdaNode node) {
    }

    void visit(ModifierNode node) {
    }

    void visit(DeclNode node) {
    }

    void visit(DeclStmtNode node) {
    }

    void visit(ConstructorNode node) {
    }

    void visit(DestrctorNode node) {
    }

    void visit(EnumNode node) {
    }

    void visit(EventNode node) {
    }

    void visit(SuperListNode node) {
    }

    void visit(InterfaceNode node) {
    }

    void visit(PropertyNode node) {
    }

    void visit(StructNode node) {
    }

    void visit(TernaryNode node) {
    }

    void visit(AttributeNode node) {
    }

    void visit(CheckedNode node) {
    }

    void visit(TypeOfNode node) {
    }

    void visit(SizeOfNode node) {
    }

    void visit(UncheckedNode node) {
    }

    void visit(ConstraintNode node) {
    }

    void visit(CatchNode node) {
    }

    void visit(FinallyNode node) {
    }

    void visit(ThrowNode node) {
    }

    void visit(TryNode node) {
    }

    void visit(ByNode node) {
    }

    void visit(EqualsNode node) {
    }

    void visit(FromNode node) {
    }

    void visit(GroupNode node) {
    }

    void visit(InNode node) {
    }

    void visit(IntoNode node) {
    }

    void visit(JoinNode node) {
    }

    void visit(LetNode node) {
    }

    void visit(LinqNode node) {
    }

    void visit(OrderByNode node) {
    }

    void visit(SelectNode node) {
    }

    void visit(WhereNode node) {
    }
}

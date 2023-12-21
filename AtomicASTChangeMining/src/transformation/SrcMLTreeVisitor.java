package transformation;

import com.github.gumtreediff.tree.Tree;
import utils.Pair;
import org.eclipse.jdt.core.dom.*;

import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class SrcMLTreeVisitor {

    public List<ASTNode> transformed_nodes;

    public SrcMLTreeVisitor(){
        transformed_nodes = new ArrayList<>();
    }

    void visit(CommentNode node){
    }
    void visit(OperatorNode node){
    }
    void visit(OnNode node){
    }
    void visit(IfNode node){

    }
    void visit(NamespaceNode node){
    }
    void visit(ArgumentListNode node){
    }
    void visit(ArgumentNode node){
    }
    void visit(NameNode node){
    }
    void visit(CallNode node){
    }

    void visit(ExprNode node){
    }
    void visit(ExprStmtNode node){
    }
    void visit(LiteralNode node){
    }
    void visit(UnitNode node){
    }
    void visit(UsingNode node){
    }
    void visit(ClassNode node){
    }

    void visit(BlockNode node){
    }
    void visit(FunctionNode node){
    }
    void visit(TypeNode node){
    }
    void visit(SpecifierNode node){
    }
    void visit(ParameterListNode node){
    }
    void visit(ParameterNode node){
    }

    void visit(BlockContentNode node){
    }
    void visit(EscapeNode node){
    }
    void visit(BreakNode node){
    }
    void visit(CaseNode node){
    }
    void visit(ContinueNode node){
    }
    void visit(DefaultNode node){
    }

    void visit(DoNode node){
    }
    void visit(EmptyStmtNode node){
    }
    void visit(FixedNode node){
    }
    void visit(ForNode node){
    }
    void visit(ForeachNode node){
    }
    void visit(GotoNode node){
    }

    void visit(IfStmtNode node){
    }
    void visit(LabelNode node){
    }
    void visit(LockNode node){
    }
    void visit(ReturnNode node){
    }
    void visit(SwitchNode node){
    }
    void visit(UnsafeNode node){
    }

    void visit(UsingStmtNode node){
    }
    void visit(WhileNode node){
    }
    void visit(ConditionNode node){
    }
    void visit(ControlNode node){
    }
    void visit(ElseNode node){
    }
    void visit(IncrNode node){
    }

    void visit(ThenNode node){
    }
    void visit(InitNode node){
    }
    void visit(DelegateNode node){
    }
    void visit(FunctionDeclNode node){
    }
    void visit(LambdaNode node){
    }
    void visit(ModifierNode node){
    }

    void visit(DeclNode node){
    }
    void visit(DeclStmtNode node){
    }
    void visit(ConstructorNode node){
    }
    void visit(DestrctorNode node){
    }
    void visit(EnumNode node){
    }
    void visit(EventNode node){
    }

    void visit(SuperListNode node){
    }
    void visit(InterfaceNode node){
    }
    void visit(PropertyNode node){
    }
    void visit(StructNode node){
    }
    void visit(TernaryNode node){
    }
    void visit(AttributeNode node){
    }

    void visit(CheckedNode node){
    }
    void visit(TypeOfNode node){
    }
    void visit(SizeOfNode node){
    }
    void visit(UncheckedNode node){
    }
    void visit(ConstraintNode node){
    }
    void visit(CatchNode node){
    }

    void visit(FinallyNode node){
    }
    void visit(ThrowNode node){
    }
    void visit(TryNode node){
    }
    void visit(ByNode node){
    }
    void visit(EqualsNode node){
    }
    void visit(FromNode node){
    }

    void visit(GroupNode node){
    }
    void visit(InNode node){
    }
    void visit(IntoNode node){
    }
    void visit(JoinNode node){
    }
    void visit(LetNode node){
    }
    void visit(LinqNode node){
    }

    void visit(OrderByNode node){
    }
    void visit(SelectNode node){
    }
    void visit(WhereNode node){
    }
}

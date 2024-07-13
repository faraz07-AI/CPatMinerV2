//import java.io.FileInputStream;
//import java.io.IOException;
//import java.io.ObjectInputStream;
//import java.util.HashMap;
//
//public class DeserializeDatFiles {
//
//    public static void main(String[] args) {
//        String filePath = "/Users/farazgurramkonda/IdeaProjects/CPatMinerV2/AtomicASTChangeMining/outputs/nightingaleproject/vrdr-dotnet/0cada944d553838d5f84f460626364942249c379.dat";
//
//        try (ObjectInputStream ois = new ObjectInputStream(new FileInputStream(filePath))) {
//            HashMap<?, ?> data = (HashMap<?, ?>) ois.readObject();
//
//            // Print or process the data
//            System.out.println("Deserialized Data: " + data);
//
//            // You can now process the data and merge it with other data as needed
//        } catch (IOException | ClassNotFoundException e) {
//            e.printStackTrace();
//        }
//    }
//}
//
//import main.pruneDoubleEdges;
//import change.ChangeGraph;
////ChangeGraph
//import java.io.File;
//import java.io.FileInputStream;
//import java.io.IOException;
//import java.io.ObjectInputStream;
//import java.util.HashMap;
//
//public class DeserializeAllFiles {
//    public static void main(String[] args) {
//        String folderPath = "/Users/farazgurramkonda/IdeaProjects/CPatMinerV2/AtomicASTChangeMining/outputs/leapmotion/Paint";
//        File folder = new File(folderPath);
//        File[] listOfFiles = folder.listFiles((dir, name) -> name.endsWith(".dat"));
//
//        HashMap<String,ChangeGraph> allGraphs = new HashMap<>();
//
//        if (listOfFiles != null) {
//            for (File file : listOfFiles) {
//                try (ObjectInputStream ois = new ObjectInputStream(new FileInputStream(file))) {
//                    HashMap<?, ?> data = (HashMap<?, ?>) ois.readObject();
//                    for (Object key : data.keySet()) {
//                        allGraphs.put((String) key, (ChangeGraph) data.get(key));
//                    }
//                } catch (IOException | ClassNotFoundException e) {
//                    e.printStackTrace();
//                }
//            }
//        } else {
//            System.out.println("No .dat files found in the specified directory.");
//        }
//        // Process and merge allGraphs as needed
//    }
//}
//
////// Assuming changeGraph has methods to get edges and nodes, merge them
////public class MergeGraphs {
////
////    public static changeGraph mergeGraphs(HashMap<String, changeGraph> allGraphs) {
////        changeGraph mergedGraph = new changeGraph();
////
////        for (changeGraph graph : allGraphs.values()) {
////            // Assuming changeGraph has methods to add edges and nodes
////            mergedGraph.addGraph(graph);
////        }
////
////        return mergedGraph;
////    }
////}

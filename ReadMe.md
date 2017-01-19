RiChecker
=========
A utility for checking Referential Integrity between CSV files.  
Intended primarily for use in a LinqPad query which defines the relationships and paths to the CSV files.

###### Example output from LinqPad Dump():
![Example Results](https://github.com/WebDev2013/RiChecker/blob/master/images/ReadMe_ExampleOutput.jpg "Example output")

## Usage with LinqPad ##

#### Referencing RIChecker.dll
1. Copy **RIChecker.dll** to the Plugins or Extensions folder.
1. In a query hit F4 to open the Query Properties dialog
1. In **Additional References**, browse to the folder containing RiChecker.dll and open it.
1. Under **Additional Namespace Imports**, use the Pick From Assemblies link to add the namespace **RiChecker** of RiChecker.dll.

#### Set up a LinqPad query that defines the Relations and runs the checker  

1. In the query editor, change the language to 'C# Statements' or 'C# Program'.  
1. Add definitions for the relation components. (See sample code for examples)  
1. Create an RiChecker object and pass it the Config and Relations objects.  
1. Call it's Check() method.
1. LinqPad's Dump() method is called to display the results in the output pane and write them to the results folder.

## API Components ##
The API for RiChecker includes the following components:  

  1. **Parsers**  
  A Parser converts a line from a CSV file to an object with named properties, using a Func delegate. You should only parse the columns that are needed in the relation checking, generally that's all of the key or foreign-key columns.  

  2. **Schema**  
  A Schema defines a CSV file, giving it the name of the file and the Parser used to extract the keys. Note that parsers can be re-used in different Schemas if the keys are the same.  

  3. **KeySelector**  
  A KeySelector is used to define which keys are to be compared in a relation check. Note that the key names must be the same in both parent and child of the relation, since only one KeySelector is defined for each relation.

  4. **Relations**  
  A Relation defines the relationship between one (or more) parent schema and one (or more) child schema. Relations are added to the Relations collection in groups for better reporting. A Relation Group will share a common KeySelector.

  5. **Config**  
  The Config object defines the paths to the files to be tested, and the current version tags. Any config items that are relatively stable can be saved to a config.json file (in the same folder as RiChecker.dll).

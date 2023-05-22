import sys

csharp_file_path = sys.argv[1]
a = open(csharp_file_path)
csharp_data = a.read()
a.close()

last_end_comment_index = 0
while "/*" in csharp_data:
  try:
    comment_index = csharp_data.index("/*", last_end_comment_index)
  except:
    break
  end_comment_index = csharp_data.index("*/", comment_index) + 2
  comment = csharp_data[comment_index:end_comment_index]
  if not "Copyright" in comment:
    print(comment)
    comment_lines = comment.split("\n")
    summary = ""
    func_io = ""
    padding = None
    for comment_line in comment_lines:
      if " * " in comment_line:
        comment_line_split = comment_line.split(" * ")
        comment_line = comment_line_split[1]
        print(comment_line)
        if comment_line.startswith("@param"):
          param_split = comment_line.split(" ", 2)
          func_io += padding + "<param name=\""+param_split[1]+"\">" + param_split[2] + "</param>\n"
        elif comment_line.startswith("@return"):
          ret_split = comment_line.split(" ", 1)
          func_io += padding + "<returns>"+ret_split[1]+"</returns>\n"
        elif comment_line.startswith("@see"):
          pass
        else:
          if padding == None:
            padding = comment_line_split[0] + "/// "
          summary += comment_line.strip() + " "
    csharp_comment = padding + "<summary>\n" + padding + summary.rstrip() + "\n"+padding+"</summary>\n" + func_io.rstrip()

    a = csharp_data[:comment_index]
    b = csharp_data[end_comment_index:]
    csharp_data = a + csharp_comment.lstrip() + b
  print(csharp_data)		

  last_end_comment_index = end_comment_index

output = open(csharp_file_path, "w")
output.write(csharp_data)
output.close()